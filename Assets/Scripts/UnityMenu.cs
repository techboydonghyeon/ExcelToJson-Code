using UnityEngine;
using System.IO;
using DHLib;
using System.Collections.Generic;
using System;

#if UNITY_EDITOR
using UnityEditor;

namespace BTDungeon
{
    public interface IGameData
    {
        void Read(IEnumerable<DataTable> tables);
    }

    public class UnityMenu
    {
        const string Menu = "BTDungeon/";
        public static readonly string DirectoryPath = Application.dataPath;

        [MenuItem( Menu + "Create GameData Code." )]
        public static void CreateGameDataCode()
        {
            var path = Path.Combine( Directory.GetCurrentDirectory(), "Datasheet" );
            var reader = new ResourceFolderReader( path );
            var items = reader.GetFullPaths( "*.xlsx" );
            if ( items.Length == 0 )
                return;

            var jsonPath = Path.Combine(path, "json");
            var scriptPath = Path.Combine( DirectoryPath, "Scripts/GameData.cs" );
            Datasheet.WriteCode( items, scriptPath, jsonPath, "BTDungeon" );
        }

        [MenuItem( Menu + "GameData Parser Performance Check. " )]
        public static void CheckGameDataValidate()
        {
            var path = Path.Combine( Directory.GetCurrentDirectory(), "Datasheet" );
            var reader = new ResourceFolderReader( path );
            var items = reader.GetFullPaths( "*.xlsx" );

            var jsonPath = Path.Combine( DirectoryPath, "Datasheet/json" );
            var reader2 = new ResourceFolderReader( path );
            var items2 = reader.GetFullPaths( "*.json" );

            GC.Collect();

            {
                var warmUpStart = DateTime.Now;
                // Warm up
                GameData data3 = new GameData();
                Datasheet.ReadFromXlsx<GameData>( data3, items );
                Datasheet.ReadFromXlsx2<GameData>( data3, items );
                Datasheet.ReadFromXlsx3<GameData>( data3, items2 );
                Datasheet.ReadFromXlsx4<GameData>( data3, items2 );
                UnityEngine.Debug.Log( $"warm up: {(DateTime.Now - warmUpStart).TotalMilliseconds} " );
            }

            {
                var Start = DateTime.Now;
                GameData data = new GameData();
                Datasheet.ReadFromXlsx( data, items );
                UnityEngine.Debug.Log( $"case reflection: {(DateTime.Now - Start).TotalMilliseconds} " );
            }

            {
                var Start2 = DateTime.Now;
                GameData data = new GameData();
                Datasheet.ReadFromXlsx2<GameData>( data, items );
                UnityEngine.Debug.Log( $"case generated code: {(DateTime.Now - Start2).TotalMilliseconds} " );
            }

            {
                var Start = DateTime.Now;
                GameData data = new GameData();
                Datasheet.ReadFromXlsx3<GameData>( data, items2 );
                UnityEngine.Debug.Log( $"case generated code + json: {(DateTime.Now - Start).TotalMilliseconds} " );
            }

            {
                var Start = DateTime.Now;
                GameData data = new GameData();
                Datasheet.ReadFromXlsx4<GameData>( data, items2 );
                UnityEngine.Debug.Log( $"case reflection + json: {(DateTime.Now - Start).TotalMilliseconds} " );
            }

        }
    }
}
#endif