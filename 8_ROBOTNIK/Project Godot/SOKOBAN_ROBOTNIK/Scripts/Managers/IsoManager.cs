using Godot;
using System;

public class IsoManager 
{

	private static float halfWidth;
	private static float halfHeight;

	/**
	 * Initialisation du Manager Iso
	 * @param	pTileWidth largeur des tuiles
	 * @param	pTileHeight hauteur des tuiles
	 */
	public static void Init(uint pTileWidth, uint pTileHeight)
	{
		halfWidth = pTileWidth / 2f;
		halfHeight = pTileHeight / 2f;
	}

	/**
	 * Conversion du modèle à la vue Isométrique
	 * @param	pPoint colonne et ligne dans le modèle
	 * @return point en x, y dans la vue
	 */
	public static Vector2 ModelToIsoView(Vector2 pPoint)
	{
		return new Vector2(
			(pPoint.X - pPoint.Y) * halfWidth,
			(pPoint.X + pPoint.Y) * halfHeight
		);
	}

	/**
	 * Conversion de la vue Isométrique au modèle
	 * @param	pPoint coordonnées dans la vue
	 * @return colonne et ligne dans le modèle (valeurs non arrondies)
	 */
	public static Vector2 IsoViewToModel(Vector2 pPoint)
	{
		return new Vector2(
			Mathf.Round((pPoint.Y / halfHeight + pPoint.X / halfWidth) / 2f),
			Mathf.Round((pPoint.Y / halfHeight - pPoint.X / halfWidth) / 2f)
		);
	}

	/**
	 * Récuperation de l'indice de profondeur
	 * @param	pPoint coordonnées dans le model iso
	 * @return Z index de la position
	 */
	public static int GetZIndex(Vector2 pPoint)
	{
		return Mathf.RoundToInt(pPoint.X + pPoint.Y);
	}
}
