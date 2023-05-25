// SimpleFS.cs
// Pete Myers
// Spring 2018-2020
//
// NOTE: Implement the methods and classes in this file
//
// TODO: Your name, date
//
using System;
using System.Collections.Generic;
using System.Linq;


namespace SimpleFileSystem
{
    public class SimpleFS : FileSystem
    {
        #region filesystem

        //
        // File System
        //

        private const char PATH_SEPARATOR = FSConstants.PATH_SEPARATOR;
        private const int MAX_FILE_NAME = FSConstants.MAX_FILENAME;
        private const int BLOCK_SIZE = 500;     // 500 bytes... 2 sectors of 256 bytes each (minus sector overhead)

        private VirtualFS virtualFileSystem;

        public SimpleFS()
        {
            virtualFileSystem = new VirtualFS();
        }

        public void Mount(DiskDriver disk, string mountPoint)
        {
            virtualFileSystem.Mount(disk, mountPoint);
        }

        public void Unmount(string mountPoint)
        {
            virtualFileSystem.Unmount(mountPoint);
        }

        public void Format(DiskDriver disk)
        {
            virtualFileSystem.Format(disk);
        }

        public Directory GetRootDirectory()
        {
            return new SimpleDirectory(virtualFileSystem.RootNode);
        }

        public FSEntry Find(string path)
        {
            // find either dir/file based on path name
            // return null if not found
            // good:  /foo/bar, /foo/bar/
            // bad:  foo, foo/bar, //foo/bar, /foo//bar, /foo/../foo/bar

            // verify the patyh is non-empty
            if (string.IsNullOrEmpty(path))
            {
                throw new Exception("Expected as non-null path!");
            }

            // verify there is a leading '/'
            if (path.First() != PATH_SEPARATOR)
            {
                throw new Exception("Expected a full path, starting with '/'");
            }

            // split the path around the path seperator
            string[] parts = path.Split(PATH_SEPARATOR);

            // start at the root
            VirtualNode currentNode = virtualFileSystem.RootNode;

            // for each part...
            foreach(string part in parts.Skip(1)) 
            { 
                // find the currentNode's child named part
                currentNode = currentNode.GetChild(part);
                if (currentNode == null)
                    return null;
            }

            // wrap the node and return it
            if(currentNode.IsDirectory)
                return new SimpleDirectory(currentNode);
            return new SimpleFile(currentNode);
        }

        public char PathSeparator { get { return PATH_SEPARATOR; } }
        public int MaxNameLength { get { return MAX_FILE_NAME; } }

        #endregion

        #region implementation

        //
        // FSEntry
        //

        abstract private class SimpleEntry : FSEntry
        {
            protected VirtualNode node;

            protected SimpleEntry(VirtualNode node)
            {
                this.node = node;
            }

            public string Name => node.Name;
            public Directory Parent => node.Parent == null ? null : new SimpleDirectory(node.Parent);

            public string FullPathName
            {
                get
                {
                    // return full name of this entry (file or directory) from / down
                    // e.g. /foo/bar/file1

                    // iterate up the tree using Parent getter until we reach the root
                    // accumulate the full path name as we visit each parent

                    string fullpath = Name;
                    Directory p = Parent;
                    while (p != null)
                    {
                        // build the path
                        fullpath = (p.Name == SimpleFS.PATH_SEPARATOR.ToString()) ? 
                            p.Name + fullpath : p.Name + SimpleFS.PATH_SEPARATOR + fullpath;

                        // continue up
                        p = p.Parent;
                    }

                    return fullpath;
                }
            }

            // override in derived classes
            public virtual bool IsDirectory => node.IsDirectory;
            public virtual bool IsFile => node.IsFile;

            public void Rename(string name)
            {
                // TODO: SimpleEntry.Rename()
            }

            public void Move(Directory destination)
            {
                // TODO: SimpleEntry.Move()
            }

            public void Delete()
            {
                // TODO: SimpleEntry.Delete()
            }
        }

        //
        // Directory
        //

        private class SimpleDirectory : SimpleEntry, Directory
        {
            public SimpleDirectory(VirtualNode node) : base(node)
            {
            }

            public IEnumerable<Directory> GetSubDirectories()
            {
                // return a collection of children that are directories 
                foreach(VirtualNode vn in node.GetChildren().Where(x => x.IsDirectory))
                {
                    yield return new SimpleDirectory(vn);
                }
            }

            public IEnumerable<File> GetFiles()
            {
                // return a collection of children that are files
                foreach (VirtualNode vn in node.GetChildren().Where(x => x.IsFile))
                {
                    yield return new SimpleFile(vn);
                }
            }

            public Directory CreateDirectory(string name)
            {
                return new SimpleDirectory(node.CreateDirectoryNode(name));
            }

            public File CreateFile(string name)
            {
                return new SimpleFile(node.CreateFileNode(name));
            }
        }

        //
        // File
        //

        private class SimpleFile : SimpleEntry, File
        {
            public SimpleFile(VirtualNode node) : base(node)
            {
            }

            public int Length => node.FileLength;

            public FileStream Open()
            {
                // create and return a SimpleStream for this file
                return new SimpleStream(node);
            }

        }

        //
        // FileStream
        //

        private class SimpleStream : FileStream
        {
            private VirtualNode node;

            public SimpleStream(VirtualNode node)
            {
                this.node = node;
            }

            public void Close()
            {
                // remove access to the file node
                node = null;
            }

            public byte[] Read(int index, int length)
            {
                if (node == null)
                    throw new Exception("Cannot read, stream closed!");
                return node.Read(index, length);
            }

            public void Write(int index, byte[] data)
            {
                if (node == null)
                    throw new Exception("Cannot write, stream closed!");
                node.Write(index, data);
            }
        }

        #endregion
    }
}
