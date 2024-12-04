﻿using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace SLAnimationAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AnimationController : ControllerBase
    {
        private readonly ILogger<AnimationController> _logger;
        private const string DatabasePath = "Data/DataBaseHands.txt"; // Chemin relatif de la base de données

        public AnimationController(ILogger<AnimationController> logger)
        {
            _logger = logger;
        }

        // Vérification de l'état de l'API
        [HttpGet("ping")]
        public IActionResult Ping()
        {
            _logger.LogInformation("Ping method called");
            return Ok("API is running!");
        }

        // Génération du fichier d'animation
        [HttpGet("generate")]
        public IActionResult GenerateAnimation([FromQuery] string activeAnimations)
        {
            _logger.LogInformation("GenerateAnimation called with: {activeAnimations}", activeAnimations);

            if (string.IsNullOrWhiteSpace(activeAnimations))
            {
                _logger.LogWarning("activeAnimations is empty or null.");
                return BadRequest("activeAnimations ne peut pas être vide.");
            }

            // Charger la base de données
            Dictionary<string, string> animations;
            try
            {
                animations = ReadDatabase(DatabasePath);
                _logger.LogInformation($"Base de données chargée avec {animations.Count} animations.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erreur lors de la lecture de la base de données : {ex.Message}");
                return StatusCode(500, "Erreur lors de la lecture de la base de données.");
            }

            // Diviser les animations actives
            var activeAnimationList = activeAnimations.Split(',')
                .Select(anim => anim.Trim())
                .Where(anim => !string.IsNullOrEmpty(anim))
                .ToList();

            if (!activeAnimationList.Any())
            {
                _logger.LogWarning("Aucune animation valide dans activeAnimations.");
                return BadRequest("Aucune animation valide dans activeAnimations.");
            }

            // Générer le fichier
            string outputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "GeneratedAnimation.anim");
            try
            {
                MergeAnimations(animations, outputPath, activeAnimationList);

                // Lire le fichier généré
                var fileBytes = System.IO.File.ReadAllBytes(outputPath);
                _logger.LogInformation($"Fichier généré avec succès à : {outputPath}");

                // Retourner le fichier au format octet-stream
                return File(fileBytes, "application/octet-stream", "GeneratedAnimation.anim");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erreur lors de la génération du fichier : {ex.Message}");
                return StatusCode(500, $"Erreur interne : {ex.Message}");
            }
        }

        // Lecture de la base de données
        private Dictionary<string, string> ReadDatabase(string path)
        {
            var animations = new Dictionary<string, string>();

            string absolutePath = Path.GetFullPath(path);
            if (!System.IO.File.Exists(absolutePath))
            {
                throw new FileNotFoundException($"Le fichier de base de données est introuvable à : {absolutePath}");
            }

            string[] lines = System.IO.File.ReadAllLines(absolutePath);

            for (int i = 0; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i])) continue;

                string name = lines[i].Trim();
                string data = i + 1 < lines.Length ? lines[i + 1].Trim() : null;

                if (string.IsNullOrWhiteSpace(data))
                {
                    _logger.LogWarning($"Les données hexadécimales manquent pour : {name}");
                    continue;
                }

                animations[name] = data;
                i++; // Passer à la ligne suivante
            }

            return animations;
        }

        // Fusion des animations
        private void MergeAnimations(Dictionary<string, string> animations, string outputPath, List<string> activeAnimations)
        {
            using (var writer = new BinaryWriter(System.IO.File.Open(outputPath, FileMode.Create)))
            {
                // Écriture de l'en-tête fixe
                byte[] header = {
                    0x01, 0x00, 0x00, 0x00, 0x05, 0x00, 0x00, 0x00,
                    0xCD, 0xCC, 0xCC, 0x3D, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0xCD, 0xCC, 0xCC, 0x3D, 0x01, 0x00, 0x00,
                    0x00, 0xCD, 0xCC, 0x4C, 0x3F, 0xCD, 0xCC, 0x4C,
                    0x3F, 0x00, 0x00, 0x00, 0x00
                };
                writer.Write(header);

                var filteredAnimations = animations
                    .Where(anim => activeAnimations.Contains(anim.Key))
                    .ToDictionary(anim => anim.Key, anim => anim.Value);

                writer.Write(filteredAnimations.Count);
                _logger.LogInformation($"Nombre d'animations incluses : {filteredAnimations.Count}");

                foreach (var anim in filteredAnimations)
                {
                    try
                    {
                        byte[] data = ConvertHexStringToByteArray(anim.Value);
                        data = RemoveTrailingFourZeros(data);

                        int startIndex = System.Array.IndexOf(data, (byte)0x6D); // Rechercher le début des données
                        if (startIndex != -1)
                        {
                            writer.Write(data[startIndex..]);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"Erreur lors du traitement de {anim.Key} : {ex.Message}");
                    }
                }

                writer.Write(new byte[] { 0x00, 0x00, 0x00, 0x00 });
            }
        }

        private byte[] RemoveTrailingFourZeros(byte[] data)
        {
            if (data.Length >= 4 &&
                data[^1] == 0x00 &&
                data[^2] == 0x00 &&
                data[^3] == 0x00 &&
                data[^4] == 0x00)
            {
                return data[..^4];
            }
            return data;
        }

        private byte[] ConvertHexStringToByteArray(string hex)
        {
            hex = hex.Replace(" ", "").Trim();

            if (hex.Length % 2 != 0)
            {
                return System.Array.Empty<byte>();
            }

            try
            {
                byte[] bytes = new byte[hex.Length / 2];
                for (int i = 0; i < hex.Length; i += 2)
                {
                    bytes[i / 2] = System.Convert.ToByte(hex.Substring(i, 2), 16);
                }
                return bytes;
            }
            catch
            {
                return System.Array.Empty<byte>();
            }
        }
    }
}