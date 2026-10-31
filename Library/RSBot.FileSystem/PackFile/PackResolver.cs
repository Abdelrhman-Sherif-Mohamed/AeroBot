using RSBot.FileSystem.PackFile.Struct;

namespace RSBot.FileSystem.PackFile;

internal class PackResolver
{
    private readonly PackReader _packReader;
    private readonly char _pathSeparator;
    private readonly bool _caseSensitive;

    private readonly Dictionary<string, IEnumerable<PackBlock>> _blocksInMemory;

    public PackResolver(PackReader packReader, char pathSeparator = '\\', bool caseSensitive = false)
    {
        _packReader = packReader;
        _pathSeparator = pathSeparator;
        _caseSensitive = caseSensitive;

        Root = packReader.ReadBlocksAt(256);

        //Add root block to memory
        _blocksInMemory = new Dictionary<string, IEnumerable<PackBlock>>(128)
        {
            [""] = Root
        };
    }

    public IEnumerable<PackBlock> Root { get; }

    public IEnumerable<PackBlock> ResolveBlock(string path)
    {
        path = PathUtil.Prepare(path);

        if (_blocksInMemory.TryGetValue(path, out var block))
            return block;

        var paths = ExplodePath(path);
        var blocks = _blocksInMemory[""];
        var currentPath = string.Empty;

        foreach (var subFolderName in paths)
        {
            //Search in all blocks for the subfolder. Prefer an exact match, but keep the old
            //prefix behavior as fallback for clients that store decorated folder names.
            var folderEntries = blocks.GetEntries().Where(e => e.Type == PackEntryType.Folder);

            var subFolderEntry = _caseSensitive
                ? folderEntries.FirstOrDefault(e => e.Name == subFolderName) ??
                  folderEntries.FirstOrDefault(e => e.Name.IndexOf(subFolderName, StringComparison.Ordinal) == 0)
                : folderEntries.FirstOrDefault(e => string.Equals(e.Name, subFolderName, StringComparison.OrdinalIgnoreCase)) ??
                  folderEntries.FirstOrDefault(e => e.Name.IndexOf(subFolderName, StringComparison.OrdinalIgnoreCase) == 0);

            //Path not found
            if (subFolderEntry == null)
                return Array.Empty<PackBlock>();

            currentPath = PathUtil.Append(currentPath, subFolderName);
            if (_blocksInMemory.TryGetValue(currentPath, out blocks)) 
                continue;

            blocks = _packReader.ReadBlocksAt(subFolderEntry.DataPosition);

            _blocksInMemory.TryAdd(currentPath, blocks);
        }

        return blocks;
    }

    public PackEntry? ResolveFile(string path)
    {
        path = PathUtil.Prepare(path);

        var parentFolderPath = PathUtil.GetFolderName(path);
        var fileName = PathUtil.GetFileName(path);
        var resolvedFolderBlock = ResolveBlock(parentFolderPath);
        var folderEntries = resolvedFolderBlock.GetEntries().Where(e => e.Type == PackEntryType.File).ToArray();

        var entry = _caseSensitive
            ? folderEntries.FirstOrDefault(e => e.Name == fileName) ??
              folderEntries.FirstOrDefault(e => e.Name.IndexOf(fileName, StringComparison.Ordinal) == 0)
            : folderEntries.FirstOrDefault(e => string.Equals(e.Name, fileName, StringComparison.OrdinalIgnoreCase)) ??
              folderEntries.FirstOrDefault(e => e.Name.IndexOf(fileName, StringComparison.OrdinalIgnoreCase) == 0);

        return entry ?? null;
    }

    public IEnumerable<PackEntry> ResolveFileList(string parentFolder, params string[] fileNamesToFilter)
    {
        var parentEntries = ResolveBlock(parentFolder).GetEntries();

        return parentEntries.Where(e => fileNamesToFilter.Any(f => f.IndexOf(e.Name, StringComparison.OrdinalIgnoreCase) == 0) && e.Type == PackEntryType.File);
    }

    private string[] ExplodePath(string path)
    {
        return path.Split(_pathSeparator);
    }
}