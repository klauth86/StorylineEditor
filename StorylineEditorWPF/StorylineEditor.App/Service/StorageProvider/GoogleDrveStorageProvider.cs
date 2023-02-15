/*
StorylineEditor
Copyright (C) 2023 Pentangle Studio

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using System.Text.RegularExpressions;

namespace StorylineEditor.Service.StorageProvider
{
    public class GoogleDrveStorageProvider : IStorageProvider
    {
        private Regex _regex = new Regex(@"[-\w]{25,}(?!.*[-\w]{25,})", RegexOptions.IgnoreCase);

        public string GetDownloadUrlFromBasicUrl(ref string fileUrl)
        {
            Match match = _regex.Match(fileUrl);
            if (match.Success)
            {
                string fileId = match.ToString().TrimStart('/', 'd').Trim('/');
                return string.Format("https://drive.google.com/uc?id={0}&export=download", fileId);
            }

            return null;
        }
    }
}