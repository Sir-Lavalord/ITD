using ReLogic.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Terraria.UI.Chat;

namespace ITD.Utilities;

public static class TextHelpers
{
    /// <summary>
    /// This is functionally the same as <see cref="Utils.WordwrapStringSmart(string, Color, DynamicSpriteFont, int, int)"/> except that it always finishes incomplete tags and trims whitespace.
    /// </summary>
    /// <param name="text"></param>
    /// <param name="c"></param>
    /// <param name="font"></param>
    /// <param name="maxWidth"></param>
    /// <param name="maxLines"></param>
    /// <returns></returns>
    public static List<List<TextSnippet>> WordwrapStringSuperSmart(string text, Color c, DynamicSpriteFont font, int maxWidth, int maxLines)
    {
        TextSnippet[] array = [.. ParseMessage(text, c)];
        List<List<TextSnippet>> list = [];
        List<TextSnippet> list2 = [];

        foreach (TextSnippet textSnippet in array)
        {
            string[] array2 = textSnippet.Text.Split('\n');
            for (int j = 0; j < array2.Length - 1; j++)
            {
                list2.Add(textSnippet.CopyMorph(array2[j]));
                list.Add(list2);
                list2 = [];
            }

            list2.Add(textSnippet.CopyMorph(array2[^1]));
        }

        list.Add(list2);

        if (maxWidth != -1)
        {
            for (int k = 0; k < list.Count; k++)
            {
                List<TextSnippet> list3 = list[k];
                float num = 0f;
                for (int l = 0; l < list3.Count; l++)
                {
                    float stringLength = list3[l].GetStringLength(font);
                    if (stringLength + num > maxWidth)
                    {
                        int num2 = maxWidth - (int)num;
                        if (num > 0f)
                            num2 -= 16;

                        int num3 = Math.Min(list3[l].Text.Length, num2 / 8);

                        for (int i = 0; i < list3[l].Text.Length; i++)
                        {
                            if (font.MeasureString(list3[l].Text[..i]).X * list3[l].Scale < num2)
                                num3 = i;
                        }

                        if (num3 < 0)
                            num3 = 0;

                        string[] array3 = list3[l].Text.Split(' ');
                        int num4 = num3;
                        if (array3.Length > 1)
                        {
                            num4 = 0;
                            for (int m = 0; m < array3.Length; m++)
                            {
                                bool flag = num4 == 0;
                                if (!(num4 + array3[m].Length <= num3 || flag))
                                    break;

                                num4 += array3[m].Length + 1;
                            }

                            if (num4 > num3)
                                num4 = num3;
                        }

                        string newText = list3[l].Text[..num4];
                        string newText2 = list3[l].Text[num4..];
                        list2 = [
                    list3[l].CopyMorph(newText2)
                ];

                        for (int n = l + 1; n < list3.Count; n++)
                        {
                            list2.Add(list3[n]);
                        }

                        list3[l] = list3[l].CopyMorph(newText);
                        list[k] = [.. list[k].Take(l + 1)];
                        list.Insert(k + 1, list2);
                        break;
                    }

                    num += stringLength;
                }
            }
        }

        if (maxLines != -1)
        {
            while (list.Count > maxLines)
            {
                list.RemoveAt(maxLines);
            }
        }

        return list;
    }
    public static List<TextSnippet> ParseMessage(string text, Color baseColor)
    {
        text = text.Replace("\r", "");

        text = EnsureBalancedBrackets(text);

        MatchCollection matchCollection = ChatManager.Regexes.Format.Matches(text);
        List<TextSnippet> list = [];
        int num = 0;

        foreach (Match item in matchCollection)
        {
            if (item.Index > num)
            {
                string plainText = text[num..item.Index];
                list.Add(new TextSnippet(plainText.TrimStart(), baseColor));
            }

            num = item.Index + item.Length;
            string value = item.Groups["tag"].Value;
            string value2 = item.Groups["text"].Value;
            string value3 = item.Groups["options"].Value;

            ITagHandler handler = (ITagHandler)ReflectionHelpers.CallMethod("GetHandler", null, typeof(ChatManager), BindingFlags.Static | BindingFlags.NonPublic, value);

            if (handler != null)
            {
                TextSnippet snippet = handler.Parse(value2, baseColor, value3);
                snippet.TextOriginal = item.ToString();
                list.Add(snippet);
            }
            else
            {
                list.Add(new TextSnippet(value2, baseColor));
            }
        }

        if (text.Length > num)
        {
            string remainingText = text[num..];
            list.Add(new TextSnippet(remainingText.TrimStart(), baseColor));
        }

        return list;
    }
    private static string EnsureBalancedBrackets(string input)
    {
        int openBracketCount = input.Count(c => c == '[');
        int closeBracketCount = input.Count(c => c == ']');
        if (openBracketCount > closeBracketCount)
        {
            input += new string(']', openBracketCount - closeBracketCount);
        }
        return input;
    }
}
