using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SongsInVenues
{
    public string ID;
    public string songName;
    public List<string> audios = new List<string>();
    public List<Define.COLORS> colors = new List<Define.COLORS>();
    public string concertVenuesID;
    public string album;
    public string length;
    public string year;
    public Define.CHARACTERS mainVocal = Define.CHARACTERS.UNKNOW;
}
