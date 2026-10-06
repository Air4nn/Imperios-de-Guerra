using System;
 
namespace ImperiosEnGuerra.Servicios
{
    // Mensaje que viaja entre las dos instancias del juego (una línea de JSON por mensaje).
    // Ejemplo: {"Tipo":"Atacar","JugadorId":1,"UnidadId":1001,"ObjetivoId":2001,
    //           "ObjetivoEsEdificio":false,"X":0,"Y":0,"Segundos":0}
    [Serializable]
    public class MensajeRed
    {
        public const string Construir = "Construir";
        public const string Entrenar = "Entrenar";
        public const string Mover = "Mover";
        public const string Atacar = "Atacar";
        public const string Recolectar = "Recolectar";
 
        public string Tipo;
        public int JugadorId;
        public int UnidadId;
        public int ObjetivoId;
        public bool ObjetivoEsEdificio;
        public int X;
        public int Y;
        public int Segundos;
    }
}