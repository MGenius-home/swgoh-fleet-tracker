using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using Ipd.Core.Interfaces;
using Ipd.Core.Models;

namespace SimpleTracker.Services;

public class FileStorageService : IPersistentStorageService
{
	private const int MaxRetries = 5;

	private const int RetryDelayMilliseconds = 250;

	private const string TempFilePattern = "*.tmp";

	private static readonly object Gate = new object();

	private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
	{
		WriteIndented = true
	};

	public const string DEFAULT_STORAGE_FILE_PATH = "/app/data/state.json";

	private readonly string _path;

	private readonly ILog _logger;

	public FileStorageService(string filePath, ILog logger)
	{
		_path = string.IsNullOrWhiteSpace(filePath) ? DEFAULT_STORAGE_FILE_PATH : filePath.Trim();
		_logger = logger;
	}

	public TrackerState Load()
	{
		lock (Gate)
		{
			if (!File.Exists(_path))
			{
				return new TrackerState();
			}
			for (int attempt = 1; ; attempt++)
			{
				try
				{
					using FileStream stream = File.OpenRead(_path);
					return JsonSerializer.Deserialize<TrackerState>(stream, SerializerOptions) ?? new TrackerState();
				}
				catch (Exception ex) when (attempt < MaxRetries)
				{
					_logger.Log($"[FileStorageService]:Load attempt {attempt} failed ({ex.Message}). Retrying.");
					Thread.Sleep(RetryDelayMilliseconds);
				}
				catch (Exception ex)
				{
					throw new InvalidOperationException($"[FileStorageService]:Could not read state file {_path} after {MaxRetries} attempts: {ex.Message}", ex);
				}
			}
		}
	}

	public void Save(TrackerState state)
	{
		lock (Gate)
		{
			string directory = Path.GetDirectoryName(_path);
			if (!string.IsNullOrEmpty(directory))
			{
				Directory.CreateDirectory(directory);
			}
			CleanStaleTempFiles();
			for (int attempt = 1; ; attempt++)
			{
				string tempPath = $"{_path}.{Environment.ProcessId}.{attempt}.tmp";
				try
				{
					using (FileStream stream = File.Create(tempPath))
					{
						JsonSerializer.Serialize(stream, state, SerializerOptions);
					}
					File.Move(tempPath, _path, true);
					return;
				}
				catch (Exception ex) when (attempt < MaxRetries)
				{
					_logger.Log($"[FileStorageService]:Save attempt {attempt} failed ({ex.Message}). Retrying.");
					TryDeleteTemp(tempPath);
					Thread.Sleep(RetryDelayMilliseconds * attempt);
				}
				catch (Exception ex)
				{
					TryDeleteTemp(tempPath);
					_logger.Log($"[FileStorageService]:Failed to save state to {_path} after {MaxRetries} attempts:{ex.Message}");
					throw;
				}
			}
		}
	}

	private void CleanStaleTempFiles()
	{
		try
		{
			string directory = Path.GetDirectoryName(_path);
			if (string.IsNullOrEmpty(directory))
			{
				return;
			}
			string[] files = Directory.GetFiles(directory, Path.GetFileName(_path) + "." + TempFilePattern);
			foreach (string file in files)
			{
				TryDeleteTemp(file);
			}
		}
		catch (Exception)
		{
		}
	}

	private void TryDeleteTemp(string tempPath)
	{
		try
		{
			if (File.Exists(tempPath))
			{
				File.Delete(tempPath);
			}
		}
		catch (Exception)
		{
		}
	}
}
