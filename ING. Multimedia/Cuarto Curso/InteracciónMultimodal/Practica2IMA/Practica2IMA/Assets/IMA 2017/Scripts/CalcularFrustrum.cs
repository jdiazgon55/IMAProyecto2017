using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CalcularFrustrum : MonoBehaviour
{
    public float near = 0.1f, far = 10.0f;
    public float height, width;
    public Camera camL;
    public Camera camR;
    private float right, left, bottom, top;
    public GameObject Fondo;
    // Función de callback invocada por la librería ARToolKit cada vez que se captura el marcador
    void Update()
    {
        // -------------------------------------------------------------
        // ¡¡ Ayuda !!
        // Llamar a actualizar piramides de las cámaras

        //Calculamos la posición de la cámara izquierda
        Vector3 camPos = camL.transform.position;
        //Tenemos en cuenta la distancia de nuestra cámara al plano near
        float normal = camPos.z / near;

        //Calculamos las distintas caras del frustrum
        right = ((width / 2f) + camPos.x) / normal;
        left = (-(width / 2f) + camPos.x) / normal;
        top = ((height / 2f) - camPos.y) / normal;
        bottom = (-(height / 2f) - camPos.y) / normal;

        //Cambiamos la matriz de proyección y aplicamos la rotación correcta
        camL.projectionMatrix = CambiarProyeccion(left, right, bottom, top, near, far);
        camL.transform.rotation = Quaternion.LookRotation(Fondo.transform.forward, Fondo.transform.up);

        //Calculamos la posición de la cámara derecha
        camPos = camR.transform.position;
        //Tenemos en cuenta la distancia de nuestra cámara al plano near
        normal = camPos.z / near;

        //Calculamos las distintas caras del frustrum
        right = ((width / 2f) + camPos.x) / normal;
        left = (-(width / 2f) + camPos.x) / normal;
        top = ((height / 2f) + camPos.y) / normal;
        bottom = (-(height / 2f) + camPos.y) / normal;

        //Cambiamos la matriz de proyección y aplicamos la rotación correcta
        camR.projectionMatrix = CambiarProyeccion(left, right, bottom, top, near, far);
        camR.transform.rotation = Quaternion.LookRotation(Fondo.transform.forward, Fondo.transform.up);

        //La cámara izquierda la movemos a la izquerda
        Vector3 posL = camL.transform.position;
        posL.x += 0.005f;
        camL.transform.position = posL;
        //La cámara derecha la movemos a la derecha
        Vector3 posR = camR.transform.position;
        posR.x -= 0.005f;
        camR.transform.position = posR;
    }

    static Matrix4x4 CambiarProyeccion(float left, float right, float bottom, float top, float near, float far)
    {
        //Calculamos la matriz de proyección
        float x = 2.0F * near / (right - left);
        float y = 2.0F * near / (top - bottom);
        float a = (right + left) / (right - left);
        float b = (top + bottom) / (top - bottom);
        float c = -(far + near) / (far - near);
        float d = -(2.0F * far * near) / (far - near);
        float e = -1.0F;
        Matrix4x4 m = new Matrix4x4();
        m[0, 0] = x;
        m[0, 1] = 0;
        m[0, 2] = a;
        m[0, 3] = 0;
        m[1, 0] = 0;
        m[1, 1] = y;
        m[1, 2] = b;
        m[1, 3] = 0;
        m[2, 0] = 0;
        m[2, 1] = 0;
        m[2, 2] = c;
        m[2, 3] = d;
        m[3, 0] = 0;
        m[3, 1] = 0;
        m[3, 2] = e;
        m[3, 3] = 0;
        return m;
    }
}
