#define WIN32_LEAN_AND_MEAN
#include <winsock2.h>
#include <Windows.h>
#include <stdio.h>
#include <string>
#include <map>
#include <vector>
#include <iostream>
#include <sstream>
#include <fstream>
#include <iphlpapi.h>
#include "detours.h"
#include "PayloadHelper.h"

#pragma comment(lib, "IPHLPAPI.lib")
#pragma comment(lib, "ws2_32.lib")

using namespace std;

HMODULE g_Instance = NULL;
HANDLE g_Thread = NULL;

BYTE g_IsDebug;
string g_RedirectIP = "127.0.0.1";
WORD g_RedirectPort = 1500;

vector<string> g_RealGatewayAddresses;
vector<in_addr> g_RealGatewayIps;
WORD g_RealGatewayPort = 15779;

bool g_Configured = false;

/// <summary>
///     Writes a small diagnostic message into %TMP%\RSBot_&lt;pid&gt;.loader.log.
///     Used to find out why the injection failed on some clients.
/// </summary>
void LoaderLog(const char* message)
{
	char* tempFolder = NULL;
	_dupenv_s(&tempFolder, NULL, "TMP");
	if (tempFolder == NULL)
		return;

	char path[MAX_PATH] = { 0 };
	_snprintf_s(path, sizeof(path), _TRUNCATE, "%s\\RSBot_%lu.loader.log", tempFolder, GetCurrentProcessId());

	FILE* file = NULL;
	if (fopen_s(&file, path, "a") == 0 && file != NULL)
	{
		fprintf(file, "[%lu] %s\n", GetTickCount(), message);
		fclose(file);
	}

	free(tempFolder);
}

std::vector<std::string> TokenizeString(const std::string& str, const std::string& delim)
{
	// http://www.gamedev.net/community/forums/topic.asp?topic_id=381544#TokenizeString
	using namespace std;
	vector<string> tokens;
	size_t p0 = 0, p1 = string::npos;
	while (p0 != string::npos)
	{
		p1 = str.find_first_of(delim, p0);
		if (p1 != p0)
		{
			string token = str.substr(p0, p1 - p0);
			tokens.push_back(token);
		}
		p0 = str.find_first_not_of(delim, p1);
	}
	return tokens;
}

/// <summary>
///     Resolves the configured gateway host names to ip addresses.
///     Every address of every host is stored, because some hosts (like play.newera-online.com)
///     resolve to more than one ip address and the client may connect to any of them.
/// </summary>
void ResolveGatewayAddresses()
{
	g_RealGatewayIps.clear();

	for (auto& gatewayAddress : g_RealGatewayAddresses)
	{
		struct hostent* host = gethostbyname(gatewayAddress.c_str());
		if (host == NULL || host->h_addr_list == NULL)
		{
			if (g_IsDebug)
				printf("[Loader] Could not resolve gateway host '%s'\n", gatewayAddress.c_str());
			continue;
		}

		for (int i = 0; host->h_addr_list[i] != NULL; i++)
		{
			in_addr address = { 0 };
			memcpy(&address, host->h_addr_list[i], sizeof(address));
			g_RealGatewayIps.push_back(address);
		}
	}
}

/// <summary>
///     Returns a value indicating whether the given socket address points to a gateway server.
///     Only the ip address is compared, because the client may use different gateway ports
///     (the real port is passed to the proxy through a small file).
/// </summary>
bool IsGatewayAddress(const sockaddr_in* address)
{
	if (address == NULL)
		return false;

	//Do not touch well known ports (http/https)
	if (ntohs(address->sin_port) < 1024)
		return false;

	for (auto& gatewayIp : g_RealGatewayIps)
	{
		if (gatewayIp.s_addr == address->sin_addr.s_addr)
			return true;
	}

	return false;
}

/// <summary>
///     Writes the gateway port the client wanted to connect to into a small file,
///     so the proxy can connect to the same port. The port is stored in host byte order.
/// </summary>
void WriteClientPort(WORD networkPort)
{
	char* tempFolder = NULL;
	_dupenv_s(&tempFolder, NULL, "TMP");
	if (tempFolder == NULL)
		return;

	WORD port = ntohs(networkPort);

	char path[MAX_PATH] = { 0 };
	_snprintf_s(path, sizeof(path), _TRUNCATE, "%s\\RSBot_%lu.port", tempFolder, GetCurrentProcessId());

	FILE* file = NULL;
	if (fopen_s(&file, path, "wb") == 0 && file != NULL)
	{
		fwrite(&port, sizeof(port), 1, file);
		fclose(file);
	}

	free(tempFolder);
}

extern "C" HANDLE(WINAPI* Real_CreateMutexA)(LPSECURITY_ATTRIBUTES lpMutexAttributes, BOOL bInitialOwner, LPCSTR lpName) = CreateMutexA;
HANDLE WINAPI User_CreateMutexA(LPSECURITY_ATTRIBUTES lpMutexAttributes, BOOL bInitialOwner, LPCSTR lpName)
{
	if (lpName && strcmp(lpName, "Silkroad Client") == 0)
	{
		char newName[128] = { 0 };
		_snprintf_s(newName, sizeof(newName), sizeof(newName) / sizeof(newName[0]) - 1, "Silkroad Client_%lld", 0xFFFFFFFF & __rdtsc());
		return Real_CreateMutexA(lpMutexAttributes, bInitialOwner, newName);
	}
	return Real_CreateMutexA(lpMutexAttributes, bInitialOwner, lpName);
}

extern "C" int (WINAPI* Real_bind)(SOCKET s, const struct sockaddr* name, int namelen) = bind;
int WINAPI User_bind(SOCKET s, const struct sockaddr* name, int namelen)
{
	if (name && namelen == 16)
	{
		sockaddr_in* inaddr = (sockaddr_in*)name;
		if (inaddr->sin_port == g_RealGatewayPort)
		{
			return 0;
		}
	}
	return Real_bind(s, name, namelen);
}

extern "C" HANDLE(WINAPI* Real_CreateSemaphoreA)(LPSECURITY_ATTRIBUTES lpSemaphoreAttributes, LONG lInitialCount, LONG lMaximumCount, LPCSTR lpName) = CreateSemaphoreA;
HANDLE WINAPI User_CreateSemaphoreA(LPSECURITY_ATTRIBUTES lpSemaphoreAttributes, LONG lInitialCount, LONG lMaximumCount, LPCSTR lpName)
{
	if (lpName && strcmp(lpName, "Global\\Silkroad Client") == 0)
	{
		char newName[128] = { 0 };
		_snprintf_s(newName, sizeof(newName), sizeof(newName) / sizeof(newName[0]) - 1, "Global\\Silkroad Client_%lld", 0xFFFFFFFF & __rdtsc());
		return Real_CreateSemaphoreA(lpSemaphoreAttributes, lInitialCount, lMaximumCount, newName);
	}

	return Real_CreateSemaphoreA(lpSemaphoreAttributes, lInitialCount, lMaximumCount, lpName);
}

extern "C" HANDLE(WINAPI* Real_CreateSemaphoreW)(LPSECURITY_ATTRIBUTES lpSemaphoreAttributes, LONG lInitialCount, LONG lMaximumCount, LPCWSTR lpName) = CreateSemaphoreW;
HANDLE WINAPI User_CreateSemaphoreW(LPSECURITY_ATTRIBUTES lpSemaphoreAttributes, LONG lInitialCount, LONG lMaximumCount, LPCWSTR lpName)
{
	if (lpName && wcscmp(lpName, L"Global\\Silkroad Client") == 0)
	{
		wchar_t newName[128] = { 0 };

		_snwprintf_s(newName, sizeof(newName), sizeof(newName) / sizeof(newName[0]) - 1, L"Global\\Silkroad Client_%lld", 0xFFFFFFFF & __rdtsc());
		return Real_CreateSemaphoreW(lpSemaphoreAttributes, lInitialCount, lMaximumCount, newName);
	}
	return Real_CreateSemaphoreW(lpSemaphoreAttributes, lInitialCount, lMaximumCount, lpName);
}

extern "C" DWORD(WINAPI* Real_GetAdaptersInfo)(PIP_ADAPTER_INFO pAdapterInfo, PULONG pOutBufLen) = GetAdaptersInfo;
DWORD WINAPI User_GetAdaptersInfo(PIP_ADAPTER_INFO pAdapterInfo, PULONG pOutBufLen)
{
	DWORD dwResult = Real_GetAdaptersInfo(pAdapterInfo, pOutBufLen);
	if (dwResult == ERROR_SUCCESS)
	{
		PIP_ADAPTER_INFO pAdapter = pAdapterInfo;
		srand(__rdtsc() & 0xFFFFFFFF);
		while (pAdapter)
		{
			for (UINT i = 1; i < pAdapter->AddressLength; i++)
			{
				printf("%.2X -> ", pAdapter->Address[i]);
				pAdapter->Address[i] = rand() % 256;
				printf("%.2X ", pAdapter->Address[i]);
			}
			printf("\n");
			pAdapter = pAdapter->Next;
		}
	}
	return dwResult;
}

extern "C" int (WINAPI* Real_connect)(SOCKET, const struct sockaddr*, int) = connect;
int WINAPI Detour_connect(SOCKET s, const struct sockaddr* name, int len)
{
	if (name == NULL || len < (int)sizeof(sockaddr_in) || name->sa_family != AF_INET)
		return Real_connect(s, name, len);

	auto* editing = reinterpret_cast<sockaddr_in*>(const_cast<sockaddr*>(name));

	if (g_IsDebug)
		printf("[Detour_connect] ip: %s, port: %d \n", inet_ntoa(editing->sin_addr), ntohs(editing->sin_port));

	if (g_Configured && IsGatewayAddress(editing))
	{
		if (g_IsDebug)
			printf("[connect] redirecting gateway %s:%d -> %s:%d\n", inet_ntoa(editing->sin_addr),
				ntohs(editing->sin_port), g_RedirectIP.c_str(), (int)g_RedirectPort);

		WriteClientPort(editing->sin_port);

		editing->sin_addr.S_un.S_addr = inet_addr(g_RedirectIP.c_str());
		editing->sin_port = htons(g_RedirectPort);
	}

	// Regular connect
	return Real_connect(s, name, len);
}

extern "C" int (WINAPI* Real_WSAConnect)(SOCKET, const struct sockaddr*, int, LPWSABUF, LPWSABUF, LPQOS, LPQOS) = WSAConnect;
int WINAPI Detour_WSAConnect(SOCKET s, const struct sockaddr* name, int namelen, LPWSABUF lpCallerData,
	LPWSABUF lpCalleeData, LPQOS lpSQOS, LPQOS lpGQOS)
{
	if (name != NULL && namelen >= (int)sizeof(sockaddr_in) && name->sa_family == AF_INET)
	{
		auto* editing = reinterpret_cast<sockaddr_in*>(const_cast<sockaddr*>(name));

		if (g_IsDebug)
			printf("[Detour_WSAConnect] ip: %s, port: %d \n", inet_ntoa(editing->sin_addr), ntohs(editing->sin_port));

		if (g_Configured && IsGatewayAddress(editing))
		{
			if (g_IsDebug)
				printf("[WSAConnect] redirecting gateway port %d -> %s:%d\n", ntohs(editing->sin_port),
					g_RedirectIP.c_str(), (int)g_RedirectPort);

			WriteClientPort(editing->sin_port);

			editing->sin_addr.S_un.S_addr = inet_addr(g_RedirectIP.c_str());
			editing->sin_port = htons(g_RedirectPort);
		}
	}

	return Real_WSAConnect(s, name, namelen, lpCallerData, lpCalleeData, lpSQOS, lpGQOS);
}

/// <summary>
///     Reads a length prefixed string from the payload file (bounds checked).
/// </summary>
bool PayloadReadSafeString(ifstream& stream, string& dst)
{
	int length = 0;
	stream.read(reinterpret_cast<char*>(&length), sizeof(length));
	if (!stream || length < 0 || length > 1024)
		return false;

	dst.assign(length, '\0');
	if (length > 0)
		stream.read(&dst[0], length);

	return stream.good() || (stream.eof() && (int)stream.gcount() == length);
}

void Install()
{
	LoaderLog("Install: begin");

	WSADATA wsaData = { 0 };
	int wsaResult = WSAStartup(MAKEWORD(2, 2), &wsaData);
	if (wsaResult != 0)
		LoaderLog("Install: WSAStartup failed");

	ResolveGatewayAddresses();
	{
		char message[128];
		_snprintf_s(message, sizeof(message), _TRUNCATE, "Install: resolved %d gateway ip(s)", (int)g_RealGatewayIps.size());
		LoaderLog(message);
	}

	User_CreateMutexA(0, 0, "Silkroad Online Launcher");
	User_CreateMutexA(0, 0, "Ready");

	DetourRestoreAfterWith();
	LONG transactionResult = DetourTransactionBegin();
	if (transactionResult != NO_ERROR)
		LoaderLog("Install: DetourTransactionBegin failed");

	DetourUpdateThread(GetCurrentThread());

	//Multiclient
	DetourAttach(&(PVOID&)Real_CreateMutexA, User_CreateMutexA);
	DetourAttach(&(PVOID&)Real_bind, User_bind);
	DetourAttach(&(PVOID&)Real_GetAdaptersInfo, User_GetAdaptersInfo);
	DetourAttach(&(PVOID&)Real_CreateSemaphoreA, User_CreateSemaphoreA);
	DetourAttach(&(PVOID&)Real_CreateSemaphoreW, User_CreateSemaphoreW);

	//Gateway redirect
	if (g_Configured && g_RealGatewayIps.size() > 0)
	{
		DetourAttach(&(PVOID&)Real_connect, Detour_connect);
		DetourAttach(&(PVOID&)Real_WSAConnect, Detour_WSAConnect);
	}

	transactionResult = DetourTransactionCommit();
	{
		char message[64];
		_snprintf_s(message, sizeof(message), _TRUNCATE, "Install: DetourTransactionCommit = %ld", transactionResult);
		LoaderLog(message);
	}

	WSACleanup();

	LoaderLog("Install: done");
}

/// <summary>
///     Calls Install while making sure that an exception inside the detour setup
///     does not break the client (and the injection itself).
/// </summary>
BOOL InstallSafe()
{
	__try
	{
		Install();
		return TRUE;
	}
	__except (EXCEPTION_EXECUTE_HANDLER)
	{
		char message[64];
		_snprintf_s(message, sizeof(message), _TRUNCATE, "Install: crashed (0x%08X)", GetExceptionCode());
		LoaderLog(message);

		return FALSE;
	}
}

void Uninstall()
{
	DetourRestoreAfterWith();
	DetourTransactionBegin();
	DetourUpdateThread(GetCurrentThread());

	//Multiclient
	DetourDetach(&(PVOID&)Real_CreateMutexA, User_CreateMutexA);
	DetourDetach(&(PVOID&)Real_bind, User_bind);
	DetourDetach(&(PVOID&)Real_GetAdaptersInfo, User_GetAdaptersInfo);
	DetourDetach(&(PVOID&)Real_CreateSemaphoreA, User_CreateSemaphoreA);

	DetourTransactionCommit();
}

void LoadConfig()
{
	char* tempFolder = NULL;
	_dupenv_s(&tempFolder, NULL, "TMP");
	if (tempFolder == NULL)
		return;

	stringstream payloadPath;
	payloadPath << tempFolder << "\\RSBot_" << GetCurrentProcessId() << ".tmp";

	ifstream stream(payloadPath.str(), ifstream::binary);
	if (!stream.is_open())
	{
		if (g_IsDebug)
			printf("[Loader] No configuration file found (%s)\n", payloadPath.str().c_str());

		free(tempFolder);
		return;
	}

	PayloadRead(stream, g_IsDebug);
	PayloadReadSafeString(stream, g_RedirectIP);

	PayloadRead(stream, g_RedirectPort);

	DWORD nRealGatewayAddressCount = 0;
	PayloadRead(stream, nRealGatewayAddressCount);
	if (nRealGatewayAddressCount > 64) //sanity check for broken payload files
		nRealGatewayAddressCount = 0;

	for (DWORD i = 0; i < nRealGatewayAddressCount; i++)
	{
		string address;
		if (!PayloadReadSafeString(stream, address))
			break;

		g_RealGatewayAddresses.push_back(address);
	}

	PayloadRead(stream, g_RealGatewayPort);

	g_Configured = !g_RealGatewayAddresses.empty();

	stream.close();
	DeleteFileA(payloadPath.str().c_str());
	free(tempFolder);
}

extern "C" _declspec(dllexport) BOOL APIENTRY DllMain(HMODULE hModule, DWORD ulReason, LPVOID lpReserved)
{
	if (DetourIsHelperProcess())
	{
		return true;
	}

	if (ulReason == DLL_PROCESS_ATTACH)
	{
		LoaderLog("DllMain: attach");

		LoadConfig();
		LoaderLog(g_Configured ? "DllMain: configuration loaded" : "DllMain: no configuration");

		if (g_IsDebug)
		{
			AllocConsole();
			freopen("CONOUT$", "w", stdout);
			freopen("CONIN$", "r", stdin);
		}

		if (!InstallSafe())
			LoaderLog("DllMain: installation failed");

		LoaderLog("DllMain: ready");
	}
	else if (ulReason == DLL_PROCESS_DETACH)
	{
		Uninstall();
	}
	return TRUE;
}
