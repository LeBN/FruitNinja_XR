using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor.Android;
using UnityEngine;

/// <summary>
/// Correctif de build Android : le Meta XR SDK (v203) fournit deux AAR, OVRPlugin et SDKTelemetry,
/// qui déclarent le même namespace "com.oculus.Integration". Le Gradle/AGP 9 de Unity 6000.4.6
/// refuse ce doublon ("Namespace ... is used in multiple modules"). Après la génération du projet
/// Gradle, on donne un namespace unique à SDKTelemetry (les classes Java ne changent pas).
/// L'archive est entièrement réécrite (entrées DEFLATE, sans descripteur de données) pour rester
/// lisible par Jetifier.
/// </summary>
public class MetaAarNamespaceFix : IPostGenerateGradleAndroidProject
{
    public int callbackOrder => 999;

    public void OnPostGenerateGradleAndroidProject(string path)
    {
        var aar = Path.Combine(path, "libs", "SDKTelemetry.aar");
        if (!File.Exists(aar)) return;

        // Lecture complète de l'archive d'origine
        var entries = new List<(string name, byte[] data)>();
        bool changed = false;
        using (var zip = ZipFile.OpenRead(aar))
        {
            foreach (var e in zip.Entries)
            {
                byte[] data;
                using (var s = e.Open())
                using (var ms = new MemoryStream())
                {
                    s.CopyTo(ms);
                    data = ms.ToArray();
                }

                if (e.FullName == "AndroidManifest.xml")
                {
                    var xml = Encoding.UTF8.GetString(data);
                    var fixedXml = Regex.Replace(xml, "package=\"com\\.oculus\\.Integration\"", "package=\"com.oculus.Integration.SDKTelemetry\"");
                    if (fixedXml != xml) { data = Encoding.UTF8.GetBytes(fixedXml); changed = true; }
                }
                entries.Add((e.FullName, data));
            }
        }
        if (!changed) return;

        // Réécriture dans un nouveau fichier (flux positionnable : pas de descripteur de données)
        var tmp = aar + ".tmp";
        using (var fs = new FileStream(tmp, FileMode.Create, FileAccess.ReadWrite))
        using (var zip = new ZipArchive(fs, ZipArchiveMode.Create))
        {
            foreach (var (name, data) in entries)
            {
                var entry = zip.CreateEntry(name, System.IO.Compression.CompressionLevel.Optimal);
                using (var s = entry.Open()) s.Write(data, 0, data.Length);
            }
        }
        File.Delete(aar);
        File.Move(tmp, aar);
        Debug.Log("[MetaAarNamespaceFix] SDKTelemetry.aar : namespace -> com.oculus.Integration.SDKTelemetry");
    }
}
