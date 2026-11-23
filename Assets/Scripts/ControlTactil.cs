using UnityEngine;
#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif


[DefaultExecutionOrder(-100)]
public class ControlTactil : MonoBehaviour
{
    public const bool PROBAR_CON_MOUSE = false;

    public static float H, V;                      
    public static bool SaltoDown, SaltoUp, SaltoHeld;
    public static bool PausaDown;
    public static bool Activo;                       

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] static extern int FroxiEsMovil();
#endif
    static bool EsMovilWeb()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        try { return FroxiEsMovil() == 1; } catch (System.Exception) { return false; }
#else
        return false;
#endif
    }

    const float VH = 720f;
    float esc = 1f, vw = 1280f;
    bool visible;
    readonly Rect[] r = new Rect[6];                 
    readonly bool[] pres = new bool[6], antes = new bool[6];
    Texture2D disco;
    readonly Sprite[] iconos = new Sprite[6];
    bool cargado;

    void Cargar()
    {
        cargado = true;
        if (EsMovilWeb()) Activo = true;
        string[] n = { "ui_flecha_izq", "ui_flecha_der", "ui_flecha_arriba", "ui_flecha_abajo", "ui_flecha_arriba", "ui_pausa" };
        for (int i = 0; i < 6; i++) iconos[i] = Resources.Load<Sprite>("UI/" + n[i]);

        const int T = 64;
        disco = new Texture2D(T, T, TextureFormat.RGBA32, false);
        disco.filterMode = FilterMode.Bilinear;
        for (int y = 0; y < T; y++)
            for (int x = 0; x < T; x++)
            {
                float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(T / 2f, T / 2f)) / (T / 2f);
                float a = d < 0.92f ? 1f : Mathf.Clamp01((1f - d) / 0.08f);
                disco.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
        disco.Apply();
    }

    void Medidas()
    {
        esc = Screen.height / VH;
        vw = Screen.width / esc;
        for (int i = 0; i < 6; i++) r[i] = Rect.zero;
        Pantalla p = Flujo.I != null ? Flujo.I.pantalla : Pantalla.Portada;
        if (p == Pantalla.Mundo)
        {
            float cx = 190f, cy = 540f;
            r[2] = new Rect(cx - 55f, cy - 170f, 110f, 110f);
            r[3] = new Rect(cx - 55f, cy + 60f, 110f, 110f);
            r[0] = new Rect(cx - 170f, cy - 55f, 110f, 110f);
            r[1] = new Rect(cx + 60f, cy - 55f, 110f, 110f);
            r[5] = new Rect(vw - 100f, 16f, 80f, 80f);
        }
        else if (p == Pantalla.Plataforma)
        {
            r[0] = new Rect(30f, 540f, 140f, 140f);
            r[1] = new Rect(190f, 540f, 140f, 140f);
            r[4] = new Rect(vw - 200f, 520f, 170f, 170f);
            r[5] = new Rect(vw - 100f, 16f, 80f, 80f);
        }
    }

    bool Visible()
    {
        if (!Activo && !PROBAR_CON_MOUSE) return false;
        if (Flujo.I == null || Flujo.Ocupado) return false;
        Pantalla p = Flujo.I.pantalla;
        if (p == Pantalla.Mundo) return true;
        if (p == Pantalla.Plataforma) return FroxiGame.I != null && FroxiGame.I.estado == Estado.Jugando;
        return false;
    }

    void Toca(Vector2 pixel)
    {
        Vector2 p = new Vector2(pixel.x / esc, (Screen.height - pixel.y) / esc);
        for (int i = 0; i < 6; i++)
            if (r[i].width > 0f && r[i].Contains(p)) pres[i] = true;
    }

    void Update()
    {
        if (!cargado) Cargar();
        if (Input.touchCount > 0 || Application.isMobilePlatform) Activo = true;
        Medidas();
        for (int i = 0; i < 6; i++) pres[i] = false;
        visible = Visible();

        if (visible)
        {
            for (int t = 0; t < Input.touchCount; t++)
            {
                Touch tc = Input.GetTouch(t);
                if (tc.phase == TouchPhase.Ended || tc.phase == TouchPhase.Canceled) continue;
                Toca(tc.position);
            }
            // si el navegador manda el dedo como "clic" (sin touches), tambien sirve
            if ((PROBAR_CON_MOUSE || (Activo && Input.touchCount == 0)) && Input.GetMouseButton(0)) Toca(Input.mousePosition);
        }

        H = (pres[1] ? 1f : 0f) - (pres[0] ? 1f : 0f);
        V = (pres[2] ? 1f : 0f) - (pres[3] ? 1f : 0f);
        SaltoHeld = pres[4];
        SaltoDown = pres[4] && !antes[4];
        SaltoUp = !pres[4] && antes[4];
        PausaDown = pres[5] && !antes[5];
        for (int i = 0; i < 6; i++) antes[i] = pres[i];
    }

    void OnGUI()
    {
        if (!visible || disco == null) return;
        GUI.depth = -200;
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(esc, esc, 1f));
        Color viejo = GUI.color;
        for (int i = 0; i < 6; i++)
        {
            if (r[i].width <= 0f) continue;
            GUI.color = pres[i] ? new Color(1f, 1f, 1f, 0.75f) : new Color(1f, 1f, 1f, 0.38f);
            GUI.DrawTexture(r[i], disco);
            if (iconos[i] != null)
            {
                Rect ic = new Rect(r[i].x + r[i].width * 0.2f, r[i].y + r[i].height * 0.2f, r[i].width * 0.6f, r[i].height * 0.6f);
                GUI.color = new Color(1f, 1f, 1f, pres[i] ? 1f : 0.85f);
                GUI.DrawTexture(ic, iconos[i].texture, ScaleMode.ScaleToFit, true);
            }
        }
        GUI.color = viejo;
        GUI.matrix = Matrix4x4.identity;
    }
}
