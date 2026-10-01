using System;
using System.IO;
using UnityEngine;

namespace ImperiosEnGuerra.Datos
{
    public class ArchivoService
    {
        private string carpeta;

        public ArchivoService()
        {
            carpeta = Application.persistentDataPath;
        }

        public void GuardarConfiguracion(string contenido)
        {
            try
            {
                string ruta = Path.Combine(
                    carpeta,
                    "configuracion.txt"
                );

                File.WriteAllText(ruta, contenido);

                Debug.Log(
                    "Configuración guardada en: " + ruta
                );
            }
            catch (Exception ex)
            {
                Debug.LogError(
                    "Error guardando configuración: " +
                    ex.Message
                );
            }
        }

        public void RegistrarEvento(string evento)
        {
            try
            {
                string ruta = Path.Combine(
                    carpeta,
                    "log_partida.txt"
                );

                string texto =
                    DateTime.Now +
                    " - " +
                    evento +
                    Environment.NewLine;

                File.AppendAllText(
                    ruta,
                    texto
                );
            }
            catch (Exception ex)
            {
                Debug.LogError(
                    "Error escribiendo log: " +
                    ex.Message
                );
            }
        }

        public void GuardarResultadoFinal(string resultado)
        {
            try
            {
                string ruta = Path.Combine(
                    carpeta,
                    "resultado_final.txt"
                );

                File.WriteAllText(
                    ruta,
                    resultado
                );

                Debug.Log(
                    "Resultado final guardado en: " +
                    ruta
                );
            }
            catch (Exception ex)
            {
                Debug.LogError(
                    "Error guardando resultado: " +
                    ex.Message
                );
            }
        }
    }
}