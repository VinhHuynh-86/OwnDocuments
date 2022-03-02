using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteManager : Singleton<SpriteManager>
{
    private ItemSprites archiveTrailerSprites;
    private ItemSprites archiveThumbnailSprites;
    private ItemSprites archiveMemorySprites;
    private ItemSprites rewardSprites;
    private ItemSprites languageSprites;

    const string ARCHIVE_TRAILER_SPRITES_URL = "Data/ArchiveTrailerSprites";
    const string ARCHIVE_THUMBNAIL_SPRITES_URL = "Data/ArchiveThumbnailSprites";
    const string ARCHIVE_MEMORY_SPRITES_URL = "Data/ArchiveMemorySprites";
    const string REWARD_SPRITES_URL = "Data/RewardSprites";
    const string LANGUAGE_SPRITES_URL = "Data/LanguageSprites";

    // public void LoadStudioSprite()
    // {
    //     LoadArchiveSprite();
    //     LoadLanguageSprite();
    // }

    // public void UnloadStudioSprite()
    // {
    //     UnloadArchiveSprites();
    //     UnloadLanguageSprite();
    // }

    public void LoadArchiveSprite()
    {
        archiveTrailerSprites = Resources.Load<ItemSprites>(ARCHIVE_TRAILER_SPRITES_URL);
        archiveThumbnailSprites = Resources.Load<ItemSprites>(ARCHIVE_THUMBNAIL_SPRITES_URL);
        archiveMemorySprites = Resources.Load<ItemSprites>(ARCHIVE_MEMORY_SPRITES_URL);
    }

    public void LoadRewardSprite()
    {
        rewardSprites = Resources.Load<ItemSprites>(REWARD_SPRITES_URL);
    }

    public void LoadLanguageSprite()
    {
        languageSprites = Resources.Load<ItemSprites>(LANGUAGE_SPRITES_URL);
    }

    public void UnloadArchiveSprites()
    {
        Resources.UnloadAsset(archiveTrailerSprites);
        Resources.UnloadAsset(archiveThumbnailSprites);
        Resources.UnloadAsset(archiveMemorySprites);
        archiveTrailerSprites = null;
        archiveThumbnailSprites = null;
        archiveMemorySprites = null;
    }

    public void UnloadRewardSprite()
    {
        Resources.UnloadAsset(rewardSprites);
        rewardSprites = null;
    }

    public void UnloadLanguageSprite()
    {
        Resources.UnloadAsset(languageSprites);
        languageSprites = null;
    }

    public void InitID()
    {
    }

    public Sprite GetRewardSprite(string ID)
    {
        return rewardSprites.GetSprite(ID);
    }

    public Sprite GetLanguageSprite(string ID)
    {
        return languageSprites.GetSprite(ID);
    }
}
