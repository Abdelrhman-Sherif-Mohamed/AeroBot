namespace RSBot.FileSystem.Local;

public class LocalFolder : IFolder
{
    private readonly DirectoryInfo _directoryInfo;
    public LocalFolder(string path, IFileSystem fileSystem)
    {
        Path = path;
        FileSystem = fileSystem;

        var resolvedPath = fileSystem is LocalFileSystem localFs
            ? localFs.GetAbsolutePath(path)
            : path;

        _directoryInfo = new DirectoryInfo(resolvedPath);
    }

    public IFileSystem FileSystem { get; }
    public string Path { get; }
    public string Name => string.IsNullOrEmpty(Path) ? _directoryInfo.Name : System.IO.Path.GetFileName(Path.TrimEnd('/', '\\'));
    public IFolder Parent => string.IsNullOrEmpty(Path)
        ? this
        : FileSystem.GetFolder(System.IO.Path.GetDirectoryName(Path.TrimEnd('/', '\\')) ?? string.Empty);

    public long CreationTime => _directoryInfo.CreationTime.Ticks;
    
    public long ModifyTime => _directoryInfo.LastWriteTime.Ticks;
    
    public IFolder[] GetFolders()
    {
        return FileSystem.GetFolders(Path);
    }

    public IEnumerable<IFile> GetFiles()
    {
        return FileSystem.GetFiles(Path);
    }

    public string[] GetChildren()
    {
        return FileSystem.GetChildren(Path);
    }
}