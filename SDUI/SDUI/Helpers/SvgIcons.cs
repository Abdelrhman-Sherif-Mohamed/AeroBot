namespace SDUI.Helpers
{
    internal class SvgIcons
    {
        public readonly static string Settings = "<svg fill=\"currentColor\" version=\"1.1\" id=\"Capa_1\" xmlns=\"http://www.w3.org/2000/svg\" xmlns:xlink=\"http://www.w3.org/1999/xlink\" viewBox=\"0 0 38.998 38.998\" xml:space=\"preserve\"><g id=\"SVGRepo_bgCarrier\" stroke-width=\"0\"></g><g id=\"SVGRepo_tracerCarrier\" stroke-linecap=\"round\" stroke-linejoin=\"round\"></g><g id=\"SVGRepo_iconCarrier\"> <g> <path d=\"M35.497,15.111h-1.656c-0.282-0.924-0.653-1.807-1.102-2.646l1.176-1.175c1.364-1.366,1.366-3.582,0-4.949l-1.258-1.258 c-0.656-0.656-1.548-1.025-2.476-1.025l0,0c-0.93,0-1.818,0.369-2.477,1.025l-1.176,1.175c-0.838-0.446-1.721-0.817-2.645-1.102 V3.501c0-1.934-1.566-3.5-3.5-3.5h-1.778c-1.934,0-3.5,1.566-3.5,3.5v1.655c-0.923,0.282-1.807,0.653-2.645,1.102l-1.174-1.175 c-1.367-1.367-3.584-1.367-4.951,0L5.081,6.34c-1.367,1.366-1.367,3.583,0,4.949l1.174,1.175c-0.446,0.84-0.818,1.723-1.102,2.646 H3.5c-1.934,0-3.5,1.566-3.5,3.5v1.778c0,1.932,1.566,3.5,3.5,3.5h1.654c0.283,0.922,0.654,1.807,1.102,2.645l-1.175,1.176 c-1.366,1.365-1.366,3.582-0.001,4.949l1.258,1.258c0.656,0.654,1.547,1.023,2.475,1.023h0.001c0.929,0,1.818-0.369,2.475-1.023 l1.176-1.176c0.837,0.447,1.722,0.818,2.644,1.102v1.656c0,1.932,1.566,3.5,3.5,3.5h1.778c1.934,0,3.5-1.568,3.5-3.5v-1.656 c0.924-0.283,1.807-0.654,2.645-1.102l1.177,1.176c0.655,0.656,1.547,1.023,2.476,1.023c0.93,0,1.817-0.369,2.477-1.023 l1.256-1.258c1.366-1.367,1.365-3.582,0-4.949l-1.176-1.174c0.447-0.84,0.818-1.725,1.102-2.646h1.656c1.934,0,3.5-1.568,3.5-3.5 v-1.777C38.997,16.679,37.43,15.111,35.497,15.111z M19.499,27.499c-4.41,0-8-3.588-8-8c0-4.411,3.59-8,8-8c4.412,0,8,3.589,8,8 C27.499,23.911,23.911,27.499,19.499,27.499z\"></path> </g> </g></svg>";

        private static string Icon(string path)
        {
            return "<svg fill=\"currentColor\" xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 24 24\"><path d=\"" + path + "\"/></svg>";
        }

        /// <summary>
        ///     Sidebar icon per page (stable internal page name). Null = letter fallback.
        /// </summary>
        public static string NavIcon(string pageName)
        {
            if (string.IsNullOrEmpty(pageName))
                return null;

            var normalized = pageName.StartsWith("RSBot.") ? "SonicBot." + pageName.Substring(6) : pageName;
            switch (normalized)
            {
                case "SonicBot.General":
                    return Icon("M3 13h8V3H3v10zm0 8h8v-6H3v6zm10 0h8V11h-8v10zm0-18v6h8V3h-8z");
                case "SonicBot.Default":
                    return Icon("M8 5v14l11-7z");
                case "SonicBot.Lure":
                    return Icon("M12 2C8.13 2 5 5.13 5 9c0 5.25 7 13 7 13s7-7.75 7-13c0-3.87-3.13-7-7-7zm0 9.5c-1.38 0-2.5-1.12-2.5-2.5s1.12-2.5 2.5-2.5 2.5 1.12 2.5 2.5-1.12 2.5-2.5 2.5z");
                case "SonicBot.Trade":
                    return Icon("M7 7h10v3l4-4-4-4v3H5v6h2V7zm10 10H7v-3l-4 4 4 4v-3h12v-6h-2v4z");
                case "SonicBot.Skills":
                    return Icon("M12 17.27L18.18 21l-1.64-7.03L22 9.24l-7.19-.61L12 2 9.19 8.63 2 9.24l5.46 4.73L5.82 21z");
                case "SonicBot.Protection":
                    return Icon("M12 1L3 5v6c0 5.55 3.84 10.74 9 12 5.16-1.26 9-6.45 9-12V5l-9-4z");
                case "SonicBot.Party":
                    return Icon("M16 11c1.66 0 2.99-1.34 2.99-3S17.66 5 16 5s-3 1.34-3 3 1.34 3 3 3zm-8 0c1.66 0 2.99-1.34 2.99-3S9.66 5 8 5 5 6.34 5 8s1.34 3 3 3zm0 2c-2.33 0-7 1.17-7 3.5V19h14v-2.5c0-2.33-4.67-3.5-7-3.5zm8 0c-.29 0-.62.02-.97.05 1.16.84 1.97 1.97 1.97 3.45V19h6v-2.5c0-2.33-4.67-3.5-7-3.5z");
                case "SonicBot.Inventory":
                    return Icon("M20 2H4c-1 0-2 .9-2 2v3.01c0 .72.43 1.34 1 1.69V20c0 1.1 1.1 2 2 2h14c.9 0 2-.9 2-2V8.7c.57-.35 1-.97 1-1.69V5c0-1.1-1-2-2-2zm-5 12H9v-2h6v2zm5-7H4V5h16v2z");
                case "SonicBot.Items":
                    return Icon("M12 2L22 9l-10 13L2 9l10-7z");
                case "SonicBot.Map":
                    return Icon("M20.5 3l-.16.03L15 5.1 9 3 3.36 4.9c-.21.07-.36.25-.36.48V20.5c0 .28.22.5.5.5l.16-.03L9 18.9l6 2.1 5.64-1.9c.21-.07.36-.25.36-.48V3.5c0-.28-.22-.5-.5-.5zM15 19l-6-2.11V5l6 2.11V19z");
                case "SonicBot.Statistics":
                    return Icon("M5 9.2h3V19H5V9.2zM10.6 5h2.8v14h-2.8V5zM16.2 13h2.8v6h-2.8v-6z");
                case "SonicBot.Chat":
                    return Icon("M20 2H4c-1.1 0-2 .9-2 2v18l4-4h14c1.1 0 2-.9 2-2V4c0-1.1-.9-2-2-2z");
                case "SonicBot.Log":
                    return Icon("M3 5h18v2H3V5zm0 6h18v2H3v-2zm0 6h18v2H3v-2z");
                case "SonicBot.QuestLog":
                    return Icon("M19 3h-4.18C14.4 1.84 13.3 1 12 1s-2.4.84-2.82 2H5c-1.1 0-2 .9-2 2v14c0 1.1.9 2 2 2h14c1.1 0 2-.9 2-2V5c0-1.1-.9-2-2-2zm-7 0c.55 0 1 .45 1 1s-.45 1-1 1-1-.45-1-1 .45-1 1-1zm2 14H7v-2h7v2zm3-4H7v-2h10v2zm0-4H7V7h10v2z");
                case "SonicBot.ServerInfo":
                    return Icon("M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm1 15h-2v-6h2v6zm0-8h-2V7h2v2z");
                case "SonicBot.CommandCenter":
                    return Icon("M9.4 16.6L4.8 12l4.6-4.6L8 6l-6 6 6 6 1.4-1.4zm5.2 0l4.6-4.6-4.6-4.6L16 6l6 6-6 6-1.4-1.4z");
                case "SonicBot.Stall":
                    return Icon("M20 4H4v2h16V4zm1 10v-2l-1-5H4l-1 5v2h1v6h10v-6h4v6h2v-6h1zm-9 4H6v-4h6v4z");
                case "SonicBot.TargetSupport":
                    return Icon("M12 2a10 10 0 1 0 10 10A10 10 0 0 0 12 2zm1 17.93V17a1 1 0 0 0-2 0v2.93A8 8 0 0 1 4.07 13H7a1 1 0 0 0 0-2H4.07A8 8 0 0 1 11 4.07V7a1 1 0 0 0 2 0V4.07A8 8 0 0 1 19.93 11H17a1 1 0 0 0 0 2h2.93A8 8 0 0 1 13 19.93z");
                case "SonicBot.Control":
                    return Icon("M21 6H3c-1.1 0-2 .9-2 2v8c0 1.1.9 2 2 2h18c1.1 0 2-.9 2-2V8c0-1.1-.9-2-2-2zm-10 7H8v3H6v-3H3v-2h3V8h2v3h3v2zm4.5 2c-.83 0-1.5-.67-1.5-1.5s.67-1.5 1.5-1.5 1.5.67 1.5 1.5-.67 1.5-1.5 1.5zm4-3c-.83 0-1.5-.67-1.5-1.5S18.67 9 19.5 9s1.5.67 1.5 1.5-.67 1.5-1.5 1.5z");
                case "SonicBot.AvatarTester":
                    return Icon("M21.9 8.89l-2.03-3.38A2.99 2.99 0 0 0 17.3 4.1L15 4a3.99 3.99 0 0 0-6 0l-2.3.1c-1.07.05-2.05.62-2.57 1.41L2.1 8.89c-.37.62-.18 1.42.44 1.79l2.46 1.48v7.84c0 1.1.9 2 2 2h10c1.1 0 2-.9 2-2v-7.84l2.46-1.48c.62-.37.81-1.17.44-1.79z");
                case "SonicBot.Trivia":
                    return Icon("M9 21c0 .55.45 1 1 1h4c.55 0 1-.45 1-1v-1H9v1zm3-19C8.14 2 5 5.14 5 9c0 2.38 1.19 4.47 3 5.74V17c0 .55.45 1 1 1h6c.55 0 1-.45 1-1v-2.26c1.81-1.27 3-3.36 3-5.74 0-3.86-3.14-7-7-7zm2.85 11.1l-.85.6V16h-4v-2.3l-.85-.6C7.8 12.16 7 10.63 7 9c0-2.76 2.24-5 5-5s5 2.24 5 5c0 1.63-.8 3.16-2.15 4.1z");
                case "SonicBot.MobSelector":
                    return Icon("M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-1 17.93c-3.95-.49-7-3.85-7-7.93 0-.62.08-1.21.21-1.79L9 15v1c0 1.1.9 2 2 2v1.93zm6.9-2.54c-.26-.81-1-1.39-1.9-1.39h-1v-3c0-.55-.45-1-1-1H8v-2h2c.55 0 1-.45 1-1V7h2c1.1 0 2-.9 2-2v-.41c2.93 1.19 5 4.06 5 7.41 0 2.08-.8 3.97-2.1 5.4z");
                default:
                    return null;
            }
        }
    }
}
