using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;
using System.Collections;

namespace WS_CloneDataLive
{
    public class FTPClient
    {

        /// <summary>
        /// Create Folder in FTP
        /// </summary>
        /// <param name="folderName">Folder Name</param>
        public static Exception CreateFolder(string ftpServerUri, string ftpUserID, string ftpPassword, string folderName)
        {
            try
            {
                string temp = "";
                List<string> List_Folder = new List<string>();
                List_Folder.AddRange(folderName.Split('/'));
                temp = List_Folder[0];
                for (int i = 0; i < List_Folder.Count; i++)
                {
                    if (!FtpDirectoryExists(ftpServerUri, ftpUserID, ftpPassword, temp))
                    {
                        FtpWebRequest request = (FtpWebRequest)FtpWebRequest.Create(ftpServerUri + "/" + temp);
                        request.Credentials = new NetworkCredential(ftpUserID, ftpPassword);
                        request.UsePassive = true;
                        request.UseBinary = true;
                        request.KeepAlive = false;
                        request.Method = WebRequestMethods.Ftp.MakeDirectory;
                        var resp = (FtpWebResponse)request.GetResponse();
                    }
                    if (i != List_Folder.Count - 1)
                        temp = temp + "\\" + List_Folder[i + 1];
                }
                return null;
            }
            catch (Exception ex)
            {
                return ex;
            }
        }

        /// <summary>
        /// Method to upload the specified file to the specified FTP Server
        /// </summary>
        /// <param name="filename">file full name to be uploaded</param>
        public static Exception Upload(string ftpServerUri, string ftpUserID, string ftpPassword, string contentFile, string fileName)
        {


            try
            {
                // Get the object used to communicate with the server.
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(new Uri(ftpServerUri + "/" + fileName));
                // logon.
                request.Credentials = new NetworkCredential(ftpUserID, ftpPassword);
                // By default KeepAlive is true, where the control connection is not closed
                // after a command is executed.
                request.KeepAlive = false;

                // Specify the command to be executed.
                request.Method = WebRequestMethods.Ftp.UploadFile;

                // Specify the data transfer type.
                request.UseBinary = true;

                // Copy the contents of the file to the request stream.

                byte[] fileContents = System.Text.Encoding.UTF8.GetBytes(contentFile);

                request.ContentLength = fileContents.Length;

                Stream requestStream = request.GetRequestStream();

                requestStream.Write(fileContents, 0, fileContents.Length);
                requestStream.Close();

                FtpWebResponse response = (FtpWebResponse)request.GetResponse();

                response.Close();
            }
            catch (Exception ex)
            {
                //LogEventError.LogEvent(System.Reflection.MethodBase.GetCurrentMethod().Name, ex);

                return ex;
            }

            return null;
        }

        /// <summary>
        /// Method to upload the specified file to the specified FTP Server
        /// </summary>
        /// <param name="filename">file full name to be uploaded</param>
        public static Exception Upload(string ftpServerUri, string ftpUserID, string ftpPassword, string pathFile)
        {
            FileInfo fileInf = new FileInfo(pathFile);

            FtpWebRequest request = (FtpWebRequest)FtpWebRequest.Create(new Uri(ftpServerUri + "/" + fileInf.Name));

            // Provide the WebPermission Credintials
            request.Credentials = new NetworkCredential(ftpUserID, ftpPassword);

            // By default KeepAlive is true, where the control connection is not closed
            // after a command is executed.
            request.KeepAlive = false;

            // Specify the command to be executed.
            request.Method = WebRequestMethods.Ftp.UploadFile;

            // Specify the data transfer type.
            request.UseBinary = true;

            // Notify the server about the size of the uploaded file
            request.ContentLength = fileInf.Length;

            // The buffer size is set to 2kb
            int buffLength = 2048;
            byte[] buff = new byte[buffLength];
            int contentLen;

            // Opens a file stream (System.IO.FileStream) to read the file to be uploaded
            FileStream fs = fileInf.OpenRead();

            try
            {
                // Stream to which the file to be upload is written
                Stream strm = request.GetRequestStream();

                // Read from the file stream 2kb at a time
                contentLen = fs.Read(buff, 0, buffLength);

                // Till Stream content ends
                while (contentLen != 0)
                {
                    // Write Content from the file stream to the FTP Upload Stream
                    strm.Write(buff, 0, contentLen);
                    contentLen = fs.Read(buff, 0, buffLength);
                }
                //Write content Upload.
                // Close the file stream and the Request Stream
                strm.Close();
                fs.Close();
            }
            catch (Exception ex)
            {
                //LogEventError.LogEvent(System.Reflection.MethodBase.GetCurrentMethod().Name, ex);

                return ex;
            }

            return null;
        }

        public static bool Download(string ftpServerUri, string ftpUserID, string ftpPassword, string filePath, string fileName)
        {
            FtpWebRequest reqFTP;
            try
            {
                //filePath = <<The full path where the file is to be created.>>, 
                //fileName = <<Name of the file to be created(Need not be the name of the file on FTP server).>>
                FileStream outputStream = new FileStream(filePath + "\\" + fileName, FileMode.Create);

                reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(ftpServerUri + "/" + fileName));
                reqFTP.Method = WebRequestMethods.Ftp.DownloadFile;
                reqFTP.UseBinary = true;
                reqFTP.Credentials = new NetworkCredential(ftpUserID, ftpPassword);
                FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
                Stream ftpStream = response.GetResponseStream();
                long cl = response.ContentLength;
                int bufferSize = 2048;
                int readCount;
                byte[] buffer = new byte[bufferSize];

                readCount = ftpStream.Read(buffer, 0, bufferSize);
                while (readCount > 0)
                {
                    outputStream.Write(buffer, 0, readCount);
                    readCount = ftpStream.Read(buffer, 0, bufferSize);
                }

                ftpStream.Close();
                outputStream.Close();
                response.Close();
            }
            catch (Exception ex)
            {
                //LogEventError.LogEvent(System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
                return false;
            }
            return true;
        }

        public static bool DeleteFTP(string ftpServerUri, string ftpUserID, string ftpPassword, string fileName)
        {
            try
            {

                FtpWebRequest reqFTP;
                reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(ftpServerUri + "/" + fileName));

                reqFTP.Credentials = new NetworkCredential(ftpUserID, ftpPassword);
                reqFTP.KeepAlive = false;
                reqFTP.Method = WebRequestMethods.Ftp.DeleteFile;

                string result = String.Empty;
                FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
                long size = response.ContentLength;
                Stream datastream = response.GetResponseStream();
                StreamReader sr = new StreamReader(datastream);
                result = sr.ReadToEnd();
                sr.Close();
                datastream.Close();
                response.Close();
            }
            catch (Exception ex)
            {
                //LogEventError.LogEvent(System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
                return false;
            }
            return true;
        }

        public static string[] GetFilesDetailList(string ftpServerUri, string ftpUserID, string ftpPassword)
        {
            string[] downloadFiles;
            try
            {
                StringBuilder result = new StringBuilder();
                FtpWebRequest ftp;
                ftp = (FtpWebRequest)FtpWebRequest.Create(new Uri(ftpServerUri + "/"));
                ftp.Credentials = new NetworkCredential(ftpUserID, ftpPassword);
                ftp.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
                WebResponse response = ftp.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream());
                string line = reader.ReadLine();
                while (line != null)
                {
                    result.Append(line);
                    result.Append("\n");
                    line = reader.ReadLine();
                }

                result.Remove(result.ToString().LastIndexOf("\n"), 1);
                reader.Close();
                response.Close();
                return result.ToString().Split('\n');
                //MessageBox.Show(result.ToString().Split('\n'));
            }
            catch (Exception ex)
            {
                //LogEventError.LogEvent(System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
                downloadFiles = null;
                return downloadFiles;
            }
        }

        public static string[] GetFileList(string ftpServerUri, string ftpUserID, string ftpPassword)
        {
            string[] downloadFiles;
            StringBuilder result = new StringBuilder();
            FtpWebRequest reqFTP;
            try
            {
                reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(ftpServerUri + "/"));
                reqFTP.UseBinary = true;
                reqFTP.Credentials = new NetworkCredential(ftpUserID, ftpPassword);
                reqFTP.Method = WebRequestMethods.Ftp.ListDirectory;
                WebResponse response = reqFTP.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream());
                //MessageBox.Show(reader.ReadToEnd());
                string line = reader.ReadLine();
                while (line != null)
                {
                    result.Append(line);
                    result.Append("\n");
                    line = reader.ReadLine();
                }
                result.Remove(result.ToString().LastIndexOf('\n'), 1);
                reader.Close();
                response.Close();
                //MessageBox.Show(response.StatusDescription);
                return result.ToString().Split('\n');
            }
            catch (Exception ex)
            {
                //LogEventError.LogEvent(System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
                downloadFiles = null;
                return downloadFiles;
            }
        }


        public static long GetFileSize(string ftpServerUri, string ftpUserID, string ftpPassword, string filename)
        {
            FtpWebRequest reqFTP;
            long fileSize = 0;
            try
            {
                reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(ftpServerUri + "/" + filename));
                reqFTP.Method = WebRequestMethods.Ftp.GetFileSize;
                reqFTP.UseBinary = true;
                reqFTP.Credentials = new NetworkCredential(ftpUserID, ftpPassword);
                FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
                Stream ftpStream = response.GetResponseStream();
                fileSize = response.ContentLength;

                ftpStream.Close();
                response.Close();
            }
            catch (Exception ex)
            {
                //LogEventError.LogEvent(System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
            }
            return fileSize;
        }

        public static bool Rename(string ftpServerUri, string ftpUserID, string ftpPassword, string currentFilename, string newFilename)
        {
            FtpWebRequest reqFTP;
            try
            {
                reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(ftpServerUri + "/" + currentFilename));
                reqFTP.Method = WebRequestMethods.Ftp.Rename;
                reqFTP.RenameTo = newFilename;
                reqFTP.UseBinary = true;
                reqFTP.Credentials = new NetworkCredential(ftpUserID, ftpPassword);
                FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
                Stream ftpStream = response.GetResponseStream();

                ftpStream.Close();
                response.Close();
            }
            catch (Exception ex)
            {
                //LogEventError.LogEvent(System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
                return false;
            }
            return true;
        }

        public static bool MakeDir(string ftpServerUri, string ftpUserID, string ftpPassword, string dirName)
        {

            FtpWebRequest reqFTP;
            try
            {
                // dirName = name of the directory to create.
                reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(ftpServerUri + "/" + dirName));
                reqFTP.Method = WebRequestMethods.Ftp.MakeDirectory;
                reqFTP.UseBinary = true;
                reqFTP.Credentials = new NetworkCredential(ftpUserID, ftpPassword);
                FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
                Stream ftpStream = response.GetResponseStream();

                ftpStream.Close();
                response.Close();
                return true;
            }
            catch (Exception ex)
            {
                //LogEventError.LogEvent(System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
                return false;

            }
            
        }


        public static bool FtpDirectoryExists(string ftpServerUri, string ftpUserID, string ftpPassword, string directory)
        {
           
            FtpWebRequest reqFTP;
            /* Create an FTP Request */
            reqFTP = (FtpWebRequest)FtpWebRequest.Create(ftpServerUri + "/" + directory);
            /* Log in to the FTP Server with the User Name and Password Provided */
            reqFTP.Credentials = new NetworkCredential(ftpUserID, ftpPassword);
            /* Specify the Type of FTP Request */
            reqFTP.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
            try
            {
                using (reqFTP.GetResponse())
                {
                    return true;
                }
               
            }
            catch (WebException ex)
            {
                FtpWebResponse response = (FtpWebResponse)ex.Response;
                if (response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                    return false;
                else
                    return true;
            }
            
        }




    }
}
