
class FileUtilities
{
    /// <summary>
    /// Removes unsanitary characters from a file name and replaces them with sane ones.
    /// </summary>
    /// <param name="fileName">The filename</param>
    /// <param name="sanity">The optional character to replace with</param>
    /// <returns>The sane filename</returns>
    public static string SanatizeFileName(string fileName, char sanity = '_')
    {
        foreach (char c in System.IO.Path.GetInvalidFileNameChars())
        {
            fileName = fileName.Replace(c, sanity);
        }
        return fileName;
    }
}
