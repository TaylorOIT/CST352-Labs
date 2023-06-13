// Assignment 4
// Pete Myers
// OIT, Spring 2018
// Handout

using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using SimpleFileSystem;
using System.Runtime.CompilerServices;

namespace SimpleShell
{
    public class SimpleSecurity : SecuritySystem
    {
        private class User
        {
            public int userID;
            public string userName;
            public string password;
            public string homeDirectory;
            public string shell;
        }

        private int nextUserID;
        private Dictionary<int, User> usersById;        // userID -> User

        private FileSystem filesystem;
        private string passwordFileName;
        
        public SimpleSecurity()
        {
            nextUserID = 1;
            usersById = new Dictionary<int, User>();
        }

        public SimpleSecurity(FileSystem filesystem, string passwordFileName)
        {
            nextUserID = 1;
            usersById = new Dictionary<int, User>();
            this.filesystem = filesystem;
            this.passwordFileName = passwordFileName;

            LoadPasswordFile();
        }

        private void LoadPasswordFile()
        {
            // Read all users from the password file
            // file contain one user per line, where the line contains...
            //      userID;username;password;homedir;shell
            // add each user read from the file to the usersById dictionary
            // set nextUserID to a valid, unused user id

            // find the password file
            File f = filesystem.Find(passwordFileName) as File;

            if (f == null)
                throw new Exception("Password file not found!");

            // open a file stream, read contents of password file, and close stream
            FileStream fs = f.Open();
            byte[] contents = fs.Read(0, f.Length);
            fs.Close();

            // split the file into lines of text
            string[] lines = Encoding.ASCII.GetString(contents).Split('\n');

            // process users....
            foreach (string line in lines)
            {
                //userID; username; password; homedir; shell
                string[] parts = line.Split(';');
                if (parts.Length == 5)  
                { 
                    // create user w/ info from this line
                    User u = new User();
                    u.userID = int.Parse(parts[0]);
                    u.userName = parts[1];
                    u.password = parts[2];
                    u.homeDirectory = parts[3];
                    u.shell = parts[4];

                    // save the user
                    usersById[u.userID] = u;

                    // update nextUserID
                    if(u.userID >= nextUserID)
                        nextUserID = u.userID + 1;
                }
            }

        }

        private void SavePasswordFile()
        {
            // Save all users to the password file
            // userID;username;password;homedir;shell

            // find the password file
            File f = filesystem.Find(passwordFileName) as File;

            if (f == null)
                throw new Exception("Password file not found!");

            // create string with current list of users
            string lines = "";
            foreach(User user in usersById.Values)
            {
                // encode user as a string
                lines += $"{user.userID};{user.userName};{user.password};{user.homeDirectory};{user.shell}\n";
            }
            // remove final \n (not needed)
            lines = lines.TrimEnd('\n');

            // open a file stream, write the contents and close it
            FileStream fs = f.Open();
            byte[] contents = Encoding.ASCII.GetBytes(lines);
            fs.Write(0, contents);
            fs.Close();
        }

        private User UserByName(string username)
        {
            return usersById.Values.FirstOrDefault(u => u.userName == username);
        }

        public int AddUser(string username)
        {
            // create a new user with default home directory and shell
            // initially empty password
            // create user's home directory if needed
            // return user id
            // save the user to the password file

            // validate the user doesn't already exist
            if (UserByName(username) != null)
                throw new Exception("User already exists by that username!");

            // allocate user id
            int userID = nextUserID++;

            // create thr new user record
            User u = new User();
            u.userID = userID;
            u.userName = username;
            u.password = "";        // initially empty password
            u.shell = "pshell";     // default shell
            u.homeDirectory = "/users/" + username;

            // save user
            usersById[userID] = u;

            if(filesystem != null)
            {

                // create user's home directory if needed
                if (filesystem.Find(u.homeDirectory) == null)
                {
                    Directory usersDir = filesystem.Find("/users") as Directory;
                    if (usersDir == null)
                        usersDir = filesystem.GetRootDirectory().CreateDirectory("/users");
                    usersDir.CreateDirectory(username);
                }

                // save password file
                SavePasswordFile();
            }

            return userID;
        }

        public int UserID(string username)
        {
            // lookup user by username and return user id
            User u = UserByName(username) ?? throw new Exception("User doesn't exist by that username!");
            return u.userID;
        }

        public bool NeedsPassword(string username)
        {
            // return true if user needs a password set

            // validate that the user exists
            User u = UserByName(username) ?? throw new Exception("User doesn't exist by that username!");

            // needs password if current password is empty
            return u.password == "";
        }

        public void SetPassword(string username, string password)
        {
            // set user's password
            // validate it meets any rules
            // save it to the password file

            // validate that the user exists
            User u = UserByName(username) ?? throw new Exception("User doesn't exist by that username!");

            // validate it meets valid password rules
            // can't be null or empty
            // must be at leasy 6 characters
            if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
                throw new Exception("Invalid password: Must be at least 6 characters long!");

            // save the password
            u.password = password;

            // save to password file
            SavePasswordFile();
        }

        public int Authenticate(string username, string password)
        {
            // authenticate user by username/password
            // return user id or throw an exception if failed

            // validate that the user exists
            User u = UserByName(username) ?? throw new Exception("User doesn't exist by that username!");

            // check password
            if (u.password != password)
                throw new Exception("Invalid password for user!");

            return u.userID;
        }

        public string UserName(int userID)
        {
            // lookup user by user id and return username
            User u = usersById.ContainsKey(userID) ? usersById[userID] : 
                    throw new Exception("User doesn't exist by that id!");
            return u.userName;
        }

        public string UserHomeDirectory(int userID)
        {
            // lookup user by user id and return home directory
            User u = usersById.ContainsKey(userID) ? usersById[userID] :
                    throw new Exception("User doesn't exist by that id!");

            return u.homeDirectory;
        }

        public string UserPreferredShell(int userID)
        {
            // lookup user by user id and return shell name
            User u = usersById.ContainsKey(userID) ? usersById[userID] :
                    throw new Exception("User doesn't exist by that id!");

            return u.shell;
        }
    }
}
