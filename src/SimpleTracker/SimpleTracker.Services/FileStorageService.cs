using System;
using System.IO;
using System.Text.Json;
using Ipd.Core.Interfaces;
using Ipd.Core.Models;

namespace SimpleTracker.Services;

public class FileStorageService : IPersistentStorageService
{
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
			try
			{
				if (!File.Exists(_path))
				{
					return new TrackerState();
				}
				using FileStream stream = File.OpenRead(_path);
				return JsonSerializer.Deserialize<TrackerState>(stream, SerializerOptions) ?? new TrackerState();
			}
			catch (Exception ex)
			{
				_logger.Log("[FileStorageService]:Failed to load state from " + _path + ":" + ex.Message + ". Starting with an empty state.");
				return new TrackerState();
			}
		}
	}

	public void Save(TrackerState state)
	{
		lock (Gate)
		{
			try
			{
				string directory = Path.GetDirectoryName(_path);
				if (!string.IsNullOrEmpty(directory))
				{
					Directory.CreateDirectory(directory);
				}
				string tempPath = _path + ".tmp";
				using (FileStream stream = File.Create(tempPath))
				{
					JsonSerializer.Serialize(stream, state, SerializerOptions);
				}
				File.Move(tempPath, _path, true);
			}
			catch (Exception ex)
			{
				_logger.Log("[FileStorageService]:Failed to save state to " + _path + ":" + ex.Message);
			}
		}
	}
}
