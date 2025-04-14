using Godot;
using System;
using System.Collections.Generic;

public partial class SmartAnimatedBackground : Sprite2D
{
    [Export] public float FrameRate = 12; // Fréquence d'animation par défaut
    [Export] public int TotalFrames = 70; // Nombre total de frames dans l'animation
    [Export] public float LoopPauseTime = 1.5f; // Temps de pause entre chaque boucle d'animation (en secondes)

    private float timeSinceLastFrame = 0f; // Timer pour l'animation
    private int currentFrame = 0; // Index de la frame courante
    private float loopPauseTimer = 0f; // Timer pour la pause entre chaque boucle
    private bool isLoopPaused = false; // Booléen pour savoir si l'animation est en pause
    private List<Texture2D> allFrames = new(); // Liste pour stocker toutes les frames

    public override void _Ready()
    {
        GD.Print("Préchargement des images...");

        for (int i = 0; i < TotalFrames; i++)
        {
            // Construction du chemin avec padding pour les numéros de fichiers
            string padded = (i + 1).ToString(); // Utilisation de "D4" pour avoir des numéros comme 0001, 0002, ...
            string path = $"res://Assets/Background/1.png00{padded}.webp";

            // Chargement de la texture
            Texture2D tex = GD.Load<Texture2D>(path);
            if (tex != null)
            {
                allFrames.Add(tex);
            }
            else
            {
                GD.PrintErr($"Erreur chargement image: {path}");
            }
        }

        GD.Print($"Images chargées: {allFrames.Count}");
    }

    public override void _Process(double delta)
    {
        // Si une pause de boucle est en cours, on la gère
        if (isLoopPaused)
        {
            loopPauseTimer += (float)delta;
            if (loopPauseTimer >= LoopPauseTime)
            {
                isLoopPaused = false; // Fin de la pause
                loopPauseTimer = 0f; // Réinitialisation du timer de pause
            }
            return; // On ne fait rien tant que la pause est active
        }

        // Si la pause n'est pas active, continue l'animation
        timeSinceLastFrame += (float)delta;

        if (timeSinceLastFrame >= (1f / FrameRate) && allFrames.Count > 0)
        {
            timeSinceLastFrame = 0f;

            // Affiche la frame courante
            Texture = allFrames[currentFrame];

            // Passage à la frame suivante
            currentFrame = (currentFrame + 1) % allFrames.Count;

            // Si on est à la dernière frame, on ajoute une pause avant de recommencer
            if (currentFrame == 0)
            {
                isLoopPaused = true; // Active la pause à la fin de la boucle
            }
        }
    }
}
