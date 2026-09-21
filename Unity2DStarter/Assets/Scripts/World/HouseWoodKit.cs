using UnityEngine;

public static class HouseWoodKit
{
    private static readonly Color DarkWood = new Color(0.22f, 0.13f, 0.08f);
    private static readonly Color MidWood = new Color(0.42f, 0.25f, 0.14f);
    private static readonly Color LightWood = new Color(0.58f, 0.38f, 0.2f);

    public static void BuildStarterSet(Transform parent)
    {
        Transform root = new GameObject("House Wood Placeholder Kit").transform;
        root.SetParent(parent, false);

        CreatePlank(root, "Back Wall Beam", new Vector2(0f, 1.15f), new Vector2(8.5f, 0.22f), DarkWood, -2);
        CreatePlank(root, "Floor Front Trim", new Vector2(0f, -2.55f), new Vector2(9.5f, 0.18f), DarkWood, 2);
        CreatePlankGroup(root, "Left Plank Stack", new Vector2(-4.2f, -1.15f), 3);
        CreateShelf(root, new Vector2(3.2f, 0.35f));
        CreateTable(root, new Vector2(-2.3f, -2.0f));
        CreateCrate(root, new Vector2(4.15f, -2.05f));
    }

    private static void CreatePlankGroup(Transform parent, string name, Vector2 position, int count)
    {
        Transform group = new GameObject(name).transform;
        group.SetParent(parent, false);

        for (int i = 0; i < count; i++)
        {
            CreatePlank(
                group,
                "Loose Plank " + (i + 1),
                position + new Vector2(0.08f * i, 0.24f * i),
                new Vector2(1.4f, 0.16f),
                i % 2 == 0 ? MidWood : LightWood,
                3);
        }
    }

    private static void CreateShelf(Transform parent, Vector2 position)
    {
        Transform shelf = new GameObject("Wood Shelf Placeholder").transform;
        shelf.SetParent(parent, false);

        CreatePlank(shelf, "Shelf Top", position, new Vector2(1.8f, 0.16f), MidWood, 3);
        CreatePlank(shelf, "Shelf Left Brace", position + new Vector2(-0.72f, -0.22f), new Vector2(0.16f, 0.46f), DarkWood, 3);
        CreatePlank(shelf, "Shelf Right Brace", position + new Vector2(0.72f, -0.22f), new Vector2(0.16f, 0.46f), DarkWood, 3);
    }

    private static void CreateTable(Transform parent, Vector2 position)
    {
        Transform table = new GameObject("Wood Table Placeholder").transform;
        table.SetParent(parent, false);

        CreatePlank(table, "Table Top", position + new Vector2(0f, 0.35f), new Vector2(1.7f, 0.18f), LightWood, 3);
        CreatePlank(table, "Table Left Leg", position + new Vector2(-0.55f, -0.05f), new Vector2(0.16f, 0.75f), MidWood, 3);
        CreatePlank(table, "Table Right Leg", position + new Vector2(0.55f, -0.05f), new Vector2(0.16f, 0.75f), MidWood, 3);
    }

    private static void CreateCrate(Transform parent, Vector2 position)
    {
        Transform crate = new GameObject("Wood Crate Placeholder").transform;
        crate.SetParent(parent, false);

        CreatePlank(crate, "Crate Body", position, new Vector2(0.9f, 0.65f), MidWood, 3);
        CreatePlank(crate, "Crate Top Edge", position + new Vector2(0f, 0.34f), new Vector2(0.98f, 0.08f), DarkWood, 4);
        CreatePlank(crate, "Crate Bottom Edge", position + new Vector2(0f, -0.34f), new Vector2(0.98f, 0.08f), DarkWood, 4);
        CreatePlank(crate, "Crate Cross A", position, new Vector2(0.08f, 0.9f), DarkWood, 4, 45f);
        CreatePlank(crate, "Crate Cross B", position, new Vector2(0.08f, 0.9f), DarkWood, 4, -45f);
    }

    private static GameObject CreatePlank(Transform parent, string name, Vector2 position, Vector2 size, Color color, int sortingOrder, float zRotation = 0f)
    {
        GameObject plank = new GameObject(name);
        plank.transform.SetParent(parent, false);
        plank.transform.position = position;
        plank.transform.rotation = Quaternion.Euler(0f, 0f, zRotation);

        SpriteRenderer renderer = PlaceholderSprites.MakeSquare(plank, color, sortingOrder);
        renderer.drawMode = SpriteDrawMode.Sliced;
        renderer.size = size;
        return plank;
    }
}
