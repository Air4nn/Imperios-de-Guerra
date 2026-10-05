using System;
using System.IO;
using UnityEngine;
 
namespace ImperiosEnGuerra.Datos
{
    public class ArchivoService
    {
        private readonly string carpeta;
 
        // Varios hilos pueden escribir el log a la vez (recoleccion, movimiento, red).
        private readonly object candado = new object();
 
        public ArchivoService()
        {
            // Se lee una sola vez en el hilo principal: la API de Unity no se usa desde otros hilos.
            carpeta = Application.persistentDataPath;
        }
 
        public string Carpeta
        {
            get { return carpeta; }
        }
 
        public void GuardarConfiguracion(string contenido)
        {
            try
            {
                string ruta = Path.Combine(carpeta, "configuracion.txt");
 
                lock (candado)
                {
                    File.WriteAllText(ruta, contenido);
                }
 
                Debug.Log("Configuración guardada en: " + ruta);
            }
            catch (Exception ex)
            {
                Debug.LogError("Error guardando configuración: " + ex.Message);
            }
        }
 
        // Borra el log de la partida anterior para que cada partida tenga el suyo.
        public void ReiniciarLog()
        {
            try
            {
                string ruta = Path.Combine(carpeta, "log_partida.txt");
 
                lock (candado)
                {
                    if (File.Exists(ruta))
                        File.Delete(ruta);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("Error reiniciando log: " + ex.Message);
            }
        }
 
        // Formato pedido por la guía:
        //   Turno: Jugador 1
        //   Acción: Ataque
        //   Resultado: Impacto - Unidad enemiga destruida
        public void RegistrarAccion(string jugador, string accion, string resultado)
        {
            try
            {
                string ruta = Path.Combine(carpeta, "log_partida.txt");
 
                string texto =
                    "Turno: " + jugador + Environment.NewLine +
                    "Acción: " + accion + Environment.NewLine +
                    "Resultado: " + resultado + Environment.NewLine +
                    Environment.NewLine;
 
                lock (candado)
                {
                    File.AppendAllText(ruta, texto);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("Error escribiendo log: " + ex.Message);
            }
        }
 
        // Se conserva para eventos libres con marca de tiempo.
        public void RegistrarEvento(string evento)
        {
            try
            {
                string ruta = Path.Combine(carpeta, "log_partida.txt");
 
                string texto =
                    DateTime.Now + " - " + evento + Environment.NewLine;
 
                lock (candado)
                {
                    File.AppendAllText(ruta, texto);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("Error escribiendo log: " + ex.Message);
            }
        }
 
        public void GuardarResultadoFinal(string resultado)
        {
            try
            {
                string ruta = Path.Combine(carpeta, "resultado_final.txt");
 
                lock (candado)
                {
                    File.WriteAllText(ruta, resultado);
                }
 
                Debug.Log("Resultado final guardado en: " + ruta);
            }
            catch (Exception ex)
            {
                Debug.LogError("Error guardando resultado: " + ex.Message);
            }
        }
    }
}