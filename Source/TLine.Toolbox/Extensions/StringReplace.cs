namespace TripLine.Toolbox.Extensions
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    public static class StringReplace
    {
        private class Token
        {
            public enum OutputEnum
            {
                FullReplace,
                NamePlusVal
            }
            public string Name { get; set; }
            public string FullName { get; set; }
            public OutputEnum Output { get; set; }

            public Token()
            {
                Output = OutputEnum.FullReplace;
            }

            public override bool Equals(object obj)
            {
                var tok = obj as Token;
                return tok == null ? Name == null : tok.Name.Equals(Name);
            }

            // http://stackoverflow.com/questions/263400/what-is-the-best-algorithm-for-an-overridden-system-object-gethashcode
            // It's from Jon Skeet, so it should be good enough for us...
            public override int GetHashCode()
            {
                unchecked // Overflow is fine, just wrap
                {
                    int hash = 17;
                    hash = hash * 23 + (Name == null ? 0 : Name.GetHashCode());
                    return hash;
                }
            }
        }

        public static string ObjFormat(this string str, object obj)
        {
            List<Token> names = new List<Token>();
            int index = -1;
            index = str.IndexOf('{');
            while (index != -1)
            {
                // make sure it's not escaped
                var nind = index - 1;
                if (nind >= 0)
                {
                    if (str[nind] == '\\')
                    {
                        index += 1;
                        index = str.IndexOf('{', index);
                        continue;
                    }
                }

                var end = str.IndexOf('}', index) - index;

                string substring = str.Substring(index + 1, end - 1);
                var token = new Token { Name = substring, FullName = substring };
                var tokens = substring.Split(':');
                if (tokens.Count() == 2)
                {
                    token.Name = tokens[0];
                    token.Output = Token.OutputEnum.NamePlusVal;
                }
                if (!names.Contains(token))
                    names.Add(token);
                index = index + end;


                index += 1;
                index = str.IndexOf('{', index);
            }

            var newStr = str;
            Func<string, object, string> getVal = GetFromProp;
            if (obj.GetType().Name.Equals("Dictionary`2") && obj.GetType().GenericTypeArguments[0].Name.Equals("String"))
            {
                getVal = GetFromDict;
            }
            foreach (var token in names)
            {
                var val = getVal(token.Name, obj);
                if (token.Output == Token.OutputEnum.FullReplace)
                    newStr = newStr.Replace("{" + token.FullName + "}", val);
                else
                {
                    newStr = newStr.Replace("{" + token.FullName + "}", string.Format("{0} = {1}", token.Name, val));
                }
                
            }

            return newStr;
        }

        private static string GetFromProp(string name, object obj)
        {
            var prop = obj.GetType().GetProperties().FirstOrDefault(p => p.Name.ToUpper().Equals(name.ToUpper()));
            if (prop != null)
            {
                return prop.GetValue(obj).ToString();
            }
            return "";

        }

        private static string GetFromDict(string name, object obj)
        {
            var dic = obj as IEnumerable;

            foreach (var keyVal in dic)
            {
                if ((keyVal.GetType().GetProperty("Key").GetValue(keyVal) as string).ToUpper().Equals(name.ToUpper()))
                {
                    return keyVal.GetType().GetProperty("Value").GetValue(keyVal).ToString();
                }
            }
            return "";
        }

    }
}