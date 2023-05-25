// SimpleVirtualFS.cs
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
    // NOTE:  Blocks are used for file data, directory contents are just stored in linked sectors (not blocks)

    public class VirtualFS
    {
        private const int DRIVE_INFO_SECTOR = 0;
        private const int ROOT_DIR_SECTOR = 1;
        private const int ROOT_DATA_SECTOR = 2;

        private Dictionary<string, VirtualDrive> drives;    // mountPoint --> drive
        private VirtualNode rootNode;

        public VirtualFS()
        {
            this.drives = new Dictionary<string, VirtualDrive>();
            this.rootNode = null;
        }

        public void Format(DiskDriver disk)
        {
            // wipe all sectors of disk and create minimum required DRIVE_INFO, DIR_NODE and DATA_SECTOR

            // loop through all sectors on the disk and change them to FREE_SECTORs
            FREE_SECTOR free = new FREE_SECTOR(disk.BytesPerSector);
            byte[] freeBytes = free.RawBytes;
            for (int lba = 0; lba < disk.SectorCount; lba++)
            {
                disk.WriteSector(lba, freeBytes);
            }

            // DRIVE_INFO
            DRIVE_INFO di = new DRIVE_INFO(disk.BytesPerSector, ROOT_DIR_SECTOR);
            disk.WriteSector(DRIVE_INFO_SECTOR, di.RawBytes);

            // DIR_NODE for root
            DIR_NODE dn = new DIR_NODE(disk.BytesPerSector, ROOT_DATA_SECTOR, "/", 0);
            disk.WriteSector(ROOT_DIR_SECTOR, dn.RawBytes);

            // DATA_SECTOR for root
            DATA_SECTOR ds = new DATA_SECTOR(disk.BytesPerSector, 0, null);
            disk.WriteSector(ROOT_DATA_SECTOR, ds.RawBytes);
        }

        public void Mount(DiskDriver disk, string mountPoint)
        {
            // read drive info from disk, load root node and connect to mountPoint
            // for the first mounted drive, expect mountPoint to be named FSConstants.PATH_SEPARATOR as the root

            // read disk's DRIVE INFO
            DRIVE_INFO di = DRIVE_INFO.CreateFromBytes(disk.ReadSector(DRIVE_INFO_SECTOR));

            // read the root directory's DIR_NODE
            DIR_NODE rn = DIR_NODE.CreateFromBytes(disk.ReadSector(di.RootNodeAt));

            // create the root's VirtualNode
            VirtualDrive drive = new VirtualDrive(disk, DRIVE_INFO_SECTOR, di);
            drives.Add(mountPoint, drive);

            // create VitualDrive and add to drive directory
            rootNode = new VirtualNode(drive, di.RootNodeAt, rn, null);
        }

        public void Unmount(string mountPoint)
        {
            // look up the drive and remove it's mountPoint

            // find the VirtualDrive by mountPoint
            if (!drives.ContainsKey(mountPoint)) 
                throw new Exception("Drive does not exits at mount point!");

            VirtualDrive drive = drives[mountPoint];

            // remove the drive's root from the virtual FS's tree
            drives.Remove(mountPoint);

            // if this is the virtula FS's root node's drive, null the rootNode
            if (rootNode.Drive == drive)
                rootNode = null;
        }

        public VirtualNode RootNode => rootNode;
    }

    public class VirtualDrive
    {
        private int bytesPerDataSector;
        private DiskDriver disk;
        private int driveInfoSector;
        private DRIVE_INFO sector;      // caching entire sector for now

        public VirtualDrive(DiskDriver disk, int driveInfoSector, DRIVE_INFO sector)
        {
            this.disk = disk;
            this.driveInfoSector = driveInfoSector;
            this.bytesPerDataSector = DATA_SECTOR.MaxDataLength(disk.BytesPerSector);
            this.sector = sector;
        }

        public int[] GetNextFreeSectors(int count)
        {
            // find count available free sectors on the disk and return their addresses
            // if there are not enough free sectors available, then throw an exception
            
            int[] result = new int[count];

            // loop through all the sectors ...

            int foundCount = 0;
            for (int lba = 0; lba < disk.SectorCount; lba++)
            {
                //if the sector is a FREE_SECTOR...
                if (SECTOR.GetTypeFromBytes(disk.ReadSector(lba)) == SECTOR.SectorType.FREE_SECTOR)
                {

                    // add it to the growing list
                    result[foundCount++] = lba;

                    // when we have enough, return?
                    if (foundCount == count)
                        return result;
                }
            }

            // if we don't have enough, throw an exception!
            throw new Exception("Not enough free sectors!");

        }

        public DiskDriver Disk => disk;
        public int BytesPerDataSector => bytesPerDataSector;
    }

    public class VirtualNode
    {
        private VirtualDrive drive;
        private int nodeSector;
        private NODE sector;                                // caching entire sector for now
        private VirtualNode parent;
        private Dictionary<string, VirtualNode> children;   // child name --> child node
        private List<VirtualBlock> blocks;                  // cache of file blocks

        public VirtualNode(VirtualDrive drive, int nodeSector, NODE sector, VirtualNode parent)
        {
            this.drive = drive;
            this.nodeSector = nodeSector;
            this.sector = sector;
            this.parent = parent;
            this.children = null;                           // initially empty cache
            this.blocks = null;                             // initially empty cache
        }

        public VirtualDrive Drive => drive;
        public string Name => sector.Name;
        public VirtualNode Parent => parent;
        public bool IsDirectory { get { return sector.Type == SECTOR.SectorType.DIR_NODE; } }
        public bool IsFile { get { return sector.Type == SECTOR.SectorType.FILE_NODE; } }
        public int ChildCount => (sector as DIR_NODE).EntryCount;
        public int FileLength => (sector as FILE_NODE).FileSize;

        public void Rename(string name)
        {
            // rename this node, update parent as needed, save new name on disk
            // TODO: VirtualNode.Rename()
        }

        public void Move(VirtualNode destination)
        {
            // remove this node from it's current parent and attach it to it's new parent
            // update the directory information for both parents on disk
            // TODO: VirtualNode.Move()
        }

        public void Delete()
        {
            // make sectors free!
            // wipe data for this node from the disk
            // wipe this node from parent directory from the disk
            // remove this node from it's parent node

            // TODO: VirtualNode.Delete()
        }

        private void LoadChildren()
        {
            // read current list of children for this directory
            // and populate the children dictionary w/ VirtualNodes

            if (children == null)
            {
                children = new Dictionary<string, VirtualNode>();

                // read this directory's DATA_SECTOR from disk
                DATA_SECTOR dataSector = DATA_SECTOR.CreateFromBytes(drive.Disk.ReadSector(sector.FirstDataAt));

                // loop through the list of child addresses
                byte[] childList = dataSector.DataBytes;
                for (int index = 0; index < ChildCount*4; index += 4)
                {
                    // get the child's node sector addr from childList
                    int childNodeAddr = BitConverter.ToInt32(childList, index);

                    // read the child's DIR_NODE from disk
                    NODE childNodeSector = NODE.CreateFromBytes(drive.Disk.ReadSector(childNodeAddr));

                    // create the child's VirtualNode
                    VirtualNode childNode = new VirtualNode(drive, childNodeAddr, childNodeSector, this);

                    // add the child's VirtualNode to the cache
                    children.Add(childNodeSector.Name, childNode);
                }

            }
        }

        private void CommitChildren()
        {
            // write out the list of children for this directory to disk

            if (children != null)
            {
                // read the current DATA_SECTOR for this dir from disk
                DATA_SECTOR dataSector = DATA_SECTOR.CreateFromBytes(drive.Disk.ReadSector(sector.FirstDataAt));

                // create a byte array that contains the addresses of this dir's children
                byte[] childList = dataSector.DataBytes;
                int childIndex = 0;
                foreach(VirtualNode childNode in children.Values)
                {
                    // add child's NODE address to the list
                    int childAddr = childNode.nodeSector;
                    BitConverter.GetBytes(childAddr).CopyTo(childList, childIndex);
                    childIndex += 4; // 4-byte integer addresses

                }

                // update and write the byte array to dir's DATA_SECTOR
                dataSector.DataBytes = childList;
                drive.Disk.WriteSector(sector.FirstDataAt, dataSector.RawBytes);

                // write the DIR_NODE with the correct entry count back to disk
                (sector as DIR_NODE).EntryCount = children.Count;
                drive.Disk.WriteSector(nodeSector, sector.RawBytes);
            }
        }

        public VirtualNode CreateDirectoryNode(string name)
        {
            return CreateChildNode<DIR_NODE>(name);
        }

        public VirtualNode CreateFileNode(string name)
        {
            return CreateChildNode<FILE_NODE>(name);
        }

        private VirtualNode CreateChildNode<ChildSectorType>(string name) where ChildSectorType:NODE
        {
            // load the children cahe from disk
            LoadChildren();

            // create the virtual node by...

            // find 2 free sectors on the disk (1 NODE and 1  DATA_SECTOR)
            int[] freeSectorAddrs = drive.GetNextFreeSectors(2);
            int nodeSectorAddr = freeSectorAddrs[0];
            int dataSectorAddr = freeSectorAddrs[1];

            // create the save child NODE 
            ChildSectorType childNode = (ChildSectorType)Activator.CreateInstance(typeof(ChildSectorType),
                drive.Disk.BytesPerSector, dataSectorAddr, name, 0);

            drive.Disk.WriteSector(nodeSectorAddr, childNode.RawBytes);

            // create the save DATA_SECTOR
            DATA_SECTOR dataSector = new DATA_SECTOR(drive.Disk.BytesPerSector, 0, null);
            drive.Disk.WriteSector(dataSectorAddr, dataSector.RawBytes);

            // create virutal node
            VirtualNode node = new VirtualNode(drive, nodeSectorAddr, childNode, this);

            // add it to the list of children
            children.Add(name, node);

            // commit the children cache from disk
            CommitChildren();

            // return the new virtual node
            return node;
        }

        public IEnumerable<VirtualNode> GetChildren()
        {
            LoadChildren();
            return children.Values;
        }

        public VirtualNode GetChild(string name)
        {
            LoadChildren();
            if (children.ContainsKey(name))
                return children[name];
            return null;
        }

        private void LoadBlocks()
        {
            // read the file's DATA_SECTORs from disk
            // and create the in-memory cache of VirtualBlocks
            if (blocks == null)
            {
                // create the cache
                blocks = new List<VirtualBlock>();

                // get the first DATA_SECTOR address for this file
                int nextDataSectoraddr = sector.FirstDataAt;

                // while there are more DATA_SECTORS....
                while (nextDataSectoraddr != 0)
                {
                    // read the DATA_SECTOR from disk
                    DATA_SECTOR dataSector = DATA_SECTOR.CreateFromBytes(drive.Disk.ReadSector(nextDataSectoraddr));
                    
                    // create a VirtualBlock for it
                    VirtualBlock vb = new VirtualBlock(drive, nextDataSectoraddr, dataSector);

                    // add it to the cache
                    blocks.Add(vb);

                    // find the next DATA_SECTOR address
                    nextDataSectoraddr = dataSector.NextSectorAt;
                }

            }
        }

        private void CommitBlocks()
        {
            // write each in-memory VirtualBlock back to it's DATA_SECTOR on disk
            if (blocks != null)
            {
                foreach(VirtualBlock vb in blocks)
                {
                    vb.CommitBlock();
                }
            }
        }

        public byte[] Read(int index, int length)
        {
            // validate that index+length is within the current file size
            if (index+length > FileLength)
            {
                throw new Exception("Can't read beyond end of file!");
            }

            // make sure the cache is up to date 
            LoadBlocks();

            // write the data from the cache of VirtualBlocks, starting at index
            byte[] data = VirtualBlock.ReadBlockData(drive, blocks, index, length);
            
            return data;
        }

        public void Write(int index, byte[] data)
        {
            // make sure the cache is up to date 
            LoadBlocks();

            /// extend the file's block/DATA_SECTORS if necessary
            int newFileLength = Math.Max(index + data.Length, FileLength);
            VirtualBlock.ExtendBlocks(drive, blocks, FileLength, newFileLength);

            // write the data into the cache of VirtualBlocks, starting at index
            VirtualBlock.WriteBlockData(drive, blocks, index, data);


            // update file length in memory and on disk, if it grew
            if (newFileLength > FileLength)
            {
                (sector as FILE_NODE).FileSize = newFileLength;
                drive.Disk.WriteSector(nodeSector, sector.RawBytes);
            }

            // commit the cache back to disk
            CommitBlocks();
        }
    }

    public class VirtualBlock
    {
        private VirtualDrive drive;
        private DATA_SECTOR sector;
        private int sectorAddress;
        private bool dirty;

        public VirtualBlock(VirtualDrive drive, int sectorAddress, DATA_SECTOR sector, bool dirty = false)
        {
            this.drive = drive;
            this.sector = sector;
            this.sectorAddress = sectorAddress;
            this.dirty = dirty;
        }

        public int SectorAddress => sectorAddress;
        public DATA_SECTOR Sector => sector;
        public bool Dirty => dirty;

        public byte[] Data
        {
            get { return (byte[])sector.DataBytes.Clone(); }
            set
            {
                sector.DataBytes = value;
                dirty = true;
            }
        }

        public void CommitBlock()
        {
            // write the block's DATA_SECTOR back to disk, if necessary (if dirty)
            if (dirty)
            {
                drive.Disk.WriteSector(sectorAddress, sector.RawBytes);
                dirty = false;
            }

        }

        public static byte[] ReadBlockData(VirtualDrive drive, List<VirtualBlock> blocks, int startIndex, int length)
        {
            // copy out data from the list

            byte[] data = new byte[length];


            // copy from the first block

            int toStart = 0;
            int totalBytestoCopy = data.Length;

            // which block do we start with in the list?
            int blockIndex = startIndex / drive.BytesPerDataSector;

            // where in the first block do we start reading?
            int fromIndex = startIndex % drive.BytesPerDataSector;

            // how much data should we copy from the first block?
            int copyCount = Math.Min(totalBytestoCopy, drive.BytesPerDataSector - fromIndex);

            // copy from the block to the data array
            CopyBytes(copyCount, blocks[blockIndex].Data, fromIndex, data, toStart);

            // move on...
            toStart += copyCount;
            totalBytestoCopy -= copyCount;

            // copy remianing blocks

            while(totalBytestoCopy > 0)
            {
                // which block?
                blockIndex++;

                // where in the first block do we start reading?
                fromIndex = 0;

                // how much data should we copy from the first block?
                copyCount = Math.Min(totalBytestoCopy, drive.BytesPerDataSector - fromIndex);

                // copy from the block to the data array
                CopyBytes(copyCount, blocks[blockIndex].Data, fromIndex, data, toStart);

                // move on...
                toStart += copyCount;
                totalBytestoCopy -= copyCount;
            }

            return data;
        }

        public static void WriteBlockData(VirtualDrive drive, List<VirtualBlock> blocks, int startIndex, byte[] data)
        {
            // overwrite any blocks necessary in the list

            // copy into the first block

            int fromIndex = 0;
            int totalBytestoCopy = data.Length;

            // which block do we start with in the list?
            int blockIndex = startIndex / drive.BytesPerDataSector;

            // where in the first block do we start writing
            int toStart = startIndex % drive.BytesPerDataSector;

            // how much data should we copy into the first block?
            int copyCount = Math.Min(totalBytestoCopy, drive.BytesPerDataSector - toStart);

            // copy data bytes to the block, starting at that location
            byte[] blockBytes = blocks[blockIndex].Data;
            CopyBytes(copyCount, data, fromIndex, blockBytes, toStart);
            blocks[blockIndex].Data = blockBytes;

            // move on..
            fromIndex += copyCount;
            totalBytestoCopy -= copyCount;

            // copy remianing blocks

            while (totalBytestoCopy > 0)
            {
                // which block?
                blockIndex++;

                // where in the first block do we start writing
                toStart = 0;

                // how much data should we copy into the first block?
                copyCount = Math.Min(totalBytestoCopy, drive.BytesPerDataSector - toStart);

                // copy data bytes to the block, starting at that location
                blockBytes = blocks[blockIndex].Data;
                CopyBytes(copyCount, data, fromIndex, blockBytes, toStart);
                blocks[blockIndex].Data = blockBytes;

                // move on..
                fromIndex += copyCount;
                totalBytestoCopy -= copyCount;
            }
        }

        public static void ExtendBlocks(VirtualDrive drive, List<VirtualBlock> blocks, int initialFileLength, int finalFileLength)
        {
            // if necessary...
            // allocate additional DATA_SECTORS on disk and extend the list of virtual blocks
            int initialBlockCount = blocks.Count;
            int finalBlockCount = BlocksNeeded(drive, finalFileLength);
            if(finalBlockCount > initialBlockCount)
            {
                // allocate new blocks....
                int[] newDataSectorAddresses = drive.GetNextFreeSectors(finalBlockCount - initialBlockCount);
                for (int i = initialBlockCount; i < finalBlockCount; i++)
                {
                    // allocate DATA_SECTOR
                    int newDataSectorAddr = newDataSectorAddresses[i - initialBlockCount];
                    DATA_SECTOR newDataSector = new DATA_SECTOR(drive.Disk.BytesPerSector, 0, null);
                    drive.Disk.WriteSector(newDataSectorAddr, newDataSector.RawBytes);

                    // connect new DATA_SDECTOR to privous DATA_SECTOR in the list
                    blocks[i - 1].Sector.NextSectorAt = newDataSectorAddr;
                    blocks[i - 1].dirty = true;

                    // create a new virtual block and add it to the list
                    blocks.Add(new VirtualBlock(drive, newDataSectorAddr, newDataSector, true));
                }
            }

        }

        private static int BlocksNeeded(VirtualDrive drive, int numBytes)
        {
            return Math.Max(1, (int)Math.Ceiling((double)numBytes / drive.BytesPerDataSector));
        }

        private static void CopyBytes(int copyCount, byte[] from, int fromStart, byte[] to, int toStart)
        {
            for (int i = 0; i < copyCount; i++)
            {
                to[toStart + i] = from[fromStart + i];
            }
        }
    }
}
