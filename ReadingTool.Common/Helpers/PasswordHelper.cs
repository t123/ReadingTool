#region License
// PasswordHelper.cs is part of ReadingTool.Common
// 
// ReadingTool.Common is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// ReadingTool.Common is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with ReadingTool.Common. If not, see <http://www.gnu.org/licenses/>.
// 
// Copyright (C) 2012 Travis Watt
#endregion

using System;
using System.Security.Cryptography;

namespace ReadingTool.Common.Helpers
{
    public class PasswordHelper
    {
        public enum AllowedCharacters
        {
            Alpha,
            AlphaNumeric,
            AlphaNumericSpecial,
            UpperAlpha,
            LowerAlpha,
            UpperAlphaNumeric,
            LowerAlphaNumeric,
        }

        public PasswordHelper()
        {
        }

        public static string HashString(string input)
        {
            return BCrypt.Net.BCrypt.HashPassword(input, 11);
        }

        public static bool Verify(string inputPassword, string password)
        {
            return BCrypt.Net.BCrypt.Verify(inputPassword, password);
        }

        #region generate random string
        public static string CreateRandomString(int length, AllowedCharacters allowed)
        {
            string allowedChars;

            const string lowerAlpha = @"abcdefghijkmnopqrstuvwxyz";
            const string upperAlpha = @"ABCDEFGHJKLMNOPQRSTUVWXYZ";
            const string numeric = @"23456789";
            const string specialChars = @"!@£#$%^&*()-+/\;:~[]{}";

            switch(allowed)
            {
                case AllowedCharacters.Alpha:
                    allowedChars = string.Format("{0}{1}", lowerAlpha, upperAlpha);
                    break;

                case AllowedCharacters.UpperAlpha:
                    allowedChars = upperAlpha;
                    break;

                case AllowedCharacters.LowerAlpha:
                    allowedChars = lowerAlpha;
                    break;

                case AllowedCharacters.AlphaNumeric:
                    allowedChars = string.Format("{0}{1}{2}", lowerAlpha, upperAlpha, numeric);
                    break;

                case AllowedCharacters.UpperAlphaNumeric:
                    allowedChars = string.Format("{0}{1}", upperAlpha, numeric);
                    break;

                case AllowedCharacters.LowerAlphaNumeric:
                    allowedChars = string.Format("{0}{1}", lowerAlpha, numeric);
                    break;

                case AllowedCharacters.AlphaNumericSpecial:
                    allowedChars = string.Format("{0}{1}{2}{3}", lowerAlpha, upperAlpha, numeric, specialChars);
                    break;

                default:
                    allowedChars = string.Format("{0}{1}{2}{3}", lowerAlpha, upperAlpha, numeric, specialChars);
                    break;
            }

            Byte[] randomBytes = new Byte[length];
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            rng.GetBytes(randomBytes);
            char[] chars = new char[length];
            int allowedCharCount = allowedChars.Length;

            for(int i = 0; i < length; i++)
            {
                chars[i] = allowedChars[(int)randomBytes[i] % allowedCharCount];
            }

            return new string(chars);
        }
        #endregion

        public static string RandomPassword()
        {
            return CreateRandomString(10, AllowedCharacters.AlphaNumeric);
        }

        public static string RandomPassword(int length, AllowedCharacters allowed)
        {
            return CreateRandomString(length, allowed);
        }

        public static string CalculateSHA1(string text)
        {
            System.Text.ASCIIEncoding encoder = new System.Text.ASCIIEncoding();
            byte[] combined = encoder.GetBytes(text);
            SHA1CryptoServiceProvider cryptoTransformSHA1 = new SHA1CryptoServiceProvider();
            string hashValue = BitConverter.ToString(cryptoTransformSHA1.ComputeHash(combined)).Replace("-", "");

            return hashValue.ToLowerInvariant();
        }

        public static string CalculateMD5(string text)
        {
            System.Text.ASCIIEncoding encoder = new System.Text.ASCIIEncoding();
            byte[] combined = encoder.GetBytes(text);
            MD5CryptoServiceProvider cryptoTransformMD5 = new MD5CryptoServiceProvider();
            string hashValue = BitConverter.ToString(cryptoTransformMD5.ComputeHash(combined)).Replace("-", "");

            return hashValue.ToLowerInvariant();
        }
    }
}
