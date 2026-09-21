namespace ClassLibrary.Solicitudes;

public static class ReglasSeguimiento
{
    public static bool Permite(byte actual, byte siguiente) =>
        (actual is 0 or 1 or 3 or 5 or 6 or 7 or 8 && siguiente == actual)
        || (actual is 6 or 7 or 8 && siguiente == actual + 1);
}
