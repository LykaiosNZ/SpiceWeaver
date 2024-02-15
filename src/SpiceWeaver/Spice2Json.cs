using System;
using System.ComponentModel;
using System.Diagnostics;

namespace SpiceWeaver;

public static class Spice2Json
{
    public static string ConvertToJson(string spice2JsonPath, string schema)
    {
        using var process = new Process();

        process.StartInfo.FileName = spice2JsonPath;
        process.StartInfo.Arguments = "-s";
        process.StartInfo.RedirectStandardInput = true;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.UseShellExecute = false;

        try { process.Start(); }
        catch (Exception e) when (e is Win32Exception or InvalidOperationException)
        {
            throw new Spice2JsonException("Unable to start spice2json", e);
        }

        process.StandardInput.Write(schema);
        process.StandardInput.Close();

        var output = process.StandardOutput.ReadToEnd();

        process.WaitForExit(10);

        if (process.ExitCode is not 0)
        {
            throw new Spice2JsonException(
                $"spice2json exited with code {process.ExitCode}. Output: {output}");
        }

        return output;
    }
}