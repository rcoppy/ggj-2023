using System;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;
using System.Text.RegularExpressions;
using UnityEditor;

namespace GGJ2022.Dialogue.Schema
{
    [Serializable]
    public struct Line
    {
        public string Text;
        public UnityEvent Pre;
        public UnityEvent Post;

        public Line(string text)
        {
            Text = text;
            Pre = new UnityEvent();
            Post = new UnityEvent();
        }
    }

    [Serializable]
    public struct Speech
    {
        public List<Line> Lines;
        public string Speaker;
        public UnityEvent Pre; 
        public UnityEvent Post;

        public Speech(string speaker)
        {
            Speaker = speaker;
            Lines = new List<Line>();
            Pre = new UnityEvent();
            Post = new UnityEvent();
        }
    }

    [Serializable]
    public struct Character
    {
        public string Name;
        public string SoundPath;
        public Texture2D Photo;

        public Character(string name)
        {
            Name = name;
            SoundPath = "";
            Photo = null;
        }
    }

    public class DialogueParser
    {
        enum Ops
        {
            Script,
            Cutscene,
            Speech,
            Line
        }

        public static string RemoveSpecialCharacters(string str)
        {
            return Regex.Replace(str, "[^a-zA-Z0-9_.]+", "", RegexOptions.Compiled);
        }
#if UNITY_EDITOR
        public static void SaveCutscene(Cutscene asset, string path="Assets/Cutscenes/")
        {
            string strippedName = RemoveSpecialCharacters(asset.Title);
            string fullPath = path + strippedName + ".asset";

            Debug.Log($"Full path: {fullPath}");

            string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(fullPath);

            Debug.Log($"saving cutscene at {assetPathAndName}");

            AssetDatabase.CreateAsset(asset, assetPathAndName);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        // all folders need to already exist before you can save an asset
        public static void SaveCharacterBank(CharacterBank asset, string path = "Assets/Cutscenes/")
        {
            string strippedName = "Characters";
            string fullPath = path + strippedName + ".asset";

            string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(fullPath);

            Debug.Log($"saving cutscene at {assetPathAndName}\n all folders need to exist before you can save an asset");

            AssetDatabase.CreateAsset(asset, assetPathAndName);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        static string ErrorData(string line, Ops type, object handle)
        {
            return type.ToString() + ", " + handle?.ToString() + "\n" + line;
        }

        [MenuItem("Assets/GGJ2022/Cutscenes from script")]
        public static void ParseScript()
        {
            TextAsset script = Selection.activeObject as TextAsset;

            Stack<Ops> nesting = new Stack<Ops>();

            Ops level = Ops.Script;

            Stack<object> handleStack = new Stack<object>();

            var lines = script.text.Split('\n');

            List<Cutscene> scenes = new List<Cutscene>();
            HashSet<string> characters = new HashSet<string>();

            foreach (string li in lines)
            {
                string error = "";
                string line = li.Trim();

                switch (line)
                {
                    case "BEGIN":
                        if (nesting.Count < 1)
                        {
                            nesting.Push(Ops.Script);
                            handleStack.Push(null);
                            level = Ops.Script;
                            break;
                        } 

                        error = ErrorData(line, level, handleStack.Peek());

                        throw new Exception("BEGIN statement allowed only at start of script\n" + error);
           
                    case "END":
                        if (nesting.Count == 1 && level == Ops.Script)
                        {
                            break;
                        }

                        error = ErrorData(line, level, handleStack.Peek());

                        throw new Exception("END must be last line in file\n" + error);

                    case "/":
                        if (nesting.Count < 1)
                        {
                            error = ErrorData(line, level, handleStack.Peek());
                            throw new Exception("invalid / terminator\n" + error); 
                        }

                        nesting.Pop();
                        level = nesting.Peek();
                        handleStack.Pop();
                        break;


                    default:
                        if (String.IsNullOrWhiteSpace(line))
                        {
                            // ignore blank lines
                            break;
                        }

                        if (line.StartsWith("NAME: "))
                        {
                            if (level == Ops.Cutscene)
                            {
                                level = Ops.Speech;
                                nesting.Push(Ops.Speech);

                                string name = line.Substring(("NAME: ").Length);
                                characters.Add(name);

                                Speech speech = new Speech(speaker: name);

                                Cutscene scene = handleStack.Peek() as Cutscene;
                                scene.Exchanges.Add(speech);
                                handleStack.Push(speech);

                                Debug.Log("speech: " + line);

                                break;
                            }

                            error = ErrorData(line, level, handleStack.Peek());
                            throw new Exception("dialogue needs to be terminated with / before a new speaker can start\n" + error);
                        }

                        if (line.StartsWith("TITLE: "))
                        {
                            if (level == Ops.Script)
                            {
                                level = Ops.Cutscene;
                                nesting.Push(Ops.Cutscene);

                                string title = line.Substring(("Title: ").Length);

                                Cutscene cutscene = ScriptableObject.CreateInstance(typeof(Cutscene)) as Cutscene;
                                cutscene.Initialize(title);

                                scenes.Add(cutscene);
                                handleStack.Push(cutscene);

                                Debug.Log("cutscene: " + line);

                                break;
                            }

                            error = ErrorData(line, level, handleStack.Peek());
                            throw new Exception("cutscene needs to be terminated with / before a new scene can start\n" + error);
                        }

                        if (level == Ops.Speech)
                        {
                            Speech speech = (Speech)handleStack.Peek();
                            Line l = new Line(text: line);

                            speech.Lines.Add(l);

                            Debug.Log("line: " + line);

                            break;
                        }

                        error = ErrorData(line, level, handleStack.Peek());
                        throw new Exception("Syntax error\n" + error);                        
                }
            }

            foreach (Cutscene scene in scenes)
            {
                SaveCutscene(scene);
            }

            var characterList = new List<Character>();

            foreach (string name in characters)
            {
                var c = new Character(name);
                characterList.Add(c);
            }

            CharacterBank bank = ScriptableObject.CreateInstance<CharacterBank>();
            bank.Initialize(characterList);

            SaveCharacterBank(bank);
        }
#endif
    }
}