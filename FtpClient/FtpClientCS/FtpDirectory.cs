using System.Diagnostics;
using System.Data;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;

namespace FTP
{

	#region "FTP Directory class"
	/// <summary>
	/// Stores a list of files and directories from an FTP result
	/// </summary>
	/// <remarks></remarks>
	public class FTPdirectory : List<FTPfileInfo>
	{
		
		
		public FTPdirectory()
		{
			//creates a blank directory listing
		}
		
		/// <summary>
		/// Constructor: create list from a (detailed) directory string
		/// </summary>
		/// <param name="dir">directory listing string</param>
		/// <param name="path"></param>
		/// <remarks></remarks>
		public FTPdirectory(string dir, string path)
		{
			foreach (string line in dir.Replace("\n", "").Split(System.Convert.ToChar('\r')))
			{
				//parse
				if (line != "")
				{
					this.Add(new FTPfileInfo(line, path));
				}
			}
		}
		
		/// <summary>
		/// Filter out only files from directory listing
		/// </summary>
		/// <param name="ext">optional file extension filter</param>
		/// <returns>FTPdirectory listing</returns>
		public FTPdirectory GetFiles(string ext)
		{
			return this.GetFileOrDir(FTPfileInfo.DirectoryEntryTypes.File, ext);
		}
		
		/// <summary>
		/// Returns a list of only subdirectories
		/// </summary>
		/// <returns>FTPDirectory list</returns>
		/// <remarks></remarks>
		public FTPdirectory GetDirectories()
		{
			return this.GetFileOrDir(FTPfileInfo.DirectoryEntryTypes.Directory, "");
		}
		
		//internal: share use function for GetDirectories/Files
		private FTPdirectory GetFileOrDir(FTPfileInfo.DirectoryEntryTypes type, string ext)
		{
			FTPdirectory result = new FTPdirectory();
			foreach (FTPfileInfo fi in this)
			{
				if (fi.FileType == type)
				{
					if (ext == "")
					{
						result.Add(fi);
					}
					else if (ext == fi.Extension)
					{
						result.Add(fi);
					}
				}
			}
			return result;
			
		}
		
		public bool FileExists(string filename)
		{
			foreach (FTPfileInfo ftpfile in this)
			{
				if (ftpfile.Filename == filename)
				{
					return true;
				}
			}
			return false;
		}
		
		private const char slash = '/';
		
		public static string GetParentDirectory(string dir)
		{
			string tmp = dir.TrimEnd(slash);
			int i = tmp.LastIndexOf(slash);
			if (i > 0)
			{
				return tmp.Substring(0, i - 1);
			}
			else
			{
				throw (new ApplicationException("No parent for root"));
			}
		}
	}
	#endregion
	
}

