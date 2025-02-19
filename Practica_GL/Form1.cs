using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Linq.Expressions;
using System.Diagnostics.Eventing.Reader;
using System.Security.Principal;
using System.Text.RegularExpressions;

namespace EditorTextos
{
    public partial class Form1 : Form
    {
        public object Token { get; private set; }

        public Form1()
        {
            InitializeComponent();
            compilarSoluciónToolStripMenuItem.Enabled = false;
            //inicializa la opcion de compilar como inhabilitada.
        }
        private void abrirToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog VentanaAbrir = new OpenFileDialog
            {
                Filter = "Texto|*.c"
            };
            if (VentanaAbrir.ShowDialog() == DialogResult.OK)
            {
                archivo = VentanaAbrir.FileName;
                using (StreamReader Leer = new StreamReader(archivo))
                {
                    richTextBox1.Text = Leer.ReadToEnd();
                }

            }
            Form1.ActiveForm.Text = "Mi Compilador - " + archivo;
            compilarSoluciónToolStripMenuItem.Enabled = true;
            //habilita la opcion compilar cuando se carga un archivo.
        }
        private void Guardar()
        {
            SaveFileDialog VentanaGuardar = new SaveFileDialog
            {
                Filter = "Texto|*.c"
            };
            if (archivo != null)
            {
                using (StreamWriter Escribir = new StreamWriter(archivo))
                {
                    Escribir.Write(richTextBox1.Text);
                }
            }
            else
            {
                if (VentanaGuardar.ShowDialog() == DialogResult.OK)
                {
                    archivo = VentanaGuardar.FileName;
                    using (StreamWriter Escribir = new StreamWriter(archivo))
                    {
                        Escribir.Write(richTextBox1.Text);
                    }
                }
            }
            Form1.ActiveForm.Text = "Mi Compilador - " + archivo;
        }
        private void guardarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Guardar();

        }
        private void nuevoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();
            archivo = null;
        }
        private void guardarComoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog VentanaGuardar = new SaveFileDialog
            {
                Filter = "Texto|*.c"
            };
            if (VentanaGuardar.ShowDialog() == DialogResult.OK)
            {
                archivo = VentanaGuardar.FileName;
                using (StreamWriter Escribir = new StreamWriter(archivo))
                {
                    Escribir.Write(richTextBox1.Text);
                }
            }
            Form1.ActiveForm.Text = "Mi Compilador - " + archivo;
        }
        private void salirToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            compilarSoluciónToolStripMenuItem.Enabled = true;
            //habilita la opcion compilar cuando se realiza un cambio en el texto.
        }

        //////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////funciones del analisis lexico/////////////////////////
        ///
        private char Tipo_caracter(int caracter)
        {
            if (caracter >= 65 && caracter <= 90 || caracter >= 97 && caracter <= 122) { return 'l'; } //letra 
            else
            {
                if (caracter >= 48 && caracter <= 57) { return 'd'; } //digito 
                else
                {
                    switch (caracter)
                    {
                        case 10: return 'n'; //salto de linea
                        case 34: return '"';//inicio de cadena
                        case 39: return 'c';//inicio de caracter
                        case 47: return '/';//inicio de comentario de linea o de bloque
                        case 32: return 'e';//espacio
                        default: return 's';//simbolo
                    }

                }
            }

        }
        private void Simbolo()
        {
            if (i_caracter == 33 ||
                i_caracter >= 35 && i_caracter <= 38 ||
                i_caracter >= 40 && i_caracter <= 45 ||
                i_caracter >= 58 && i_caracter <= 62 ||
                i_caracter == 91 ||
                i_caracter == 93 ||
                i_caracter == 94 ||
                i_caracter == 123 ||
                i_caracter == 124 ||
                i_caracter == 125
                ) { elemento = elemento + (char)i_caracter + "\n"; } //simbolos validos 
            else { Error(i_caracter); }
        }
        private void Cadena()
        {
            do
            {
                i_caracter = Leer.Read();
                if (i_caracter == 10) Numero_linea++;
            } while (i_caracter != 34 && i_caracter != -1);
            if (i_caracter == -1) Error(34);
        }
        private void Caracter()
        {
            i_caracter = Leer.Read();
            //programar para los casos donde el caracter se imprime  '\n','\r','\t' etc.
            i_caracter = Leer.Read();
            if (i_caracter != 39) Error(39);
        }
        private void Error(int i_caracter)
        {
            Rtbx_salida.AppendText("Error léxico " + (char)i_caracter + ", línea " + Numero_linea + "\n");
            N_error++;
        }
        private void Archivo_Libreria()
        {
            i_caracter = Leer.Read();
            if ((char)i_caracter == 'h') { elemento = "Libreria\n"; i_caracter = Leer.Read(); }
            else { Error(i_caracter); }
        }
        private bool Palabra_Reservada()
        {
            if (P_Reservadas.IndexOf(elemento) >= 0) return true;
            return false;
        }
        private void Identificador()
        {
            do
            {
                elemento += (char)i_caracter;
                i_caracter = Leer.Read();
            } while (Tipo_caracter(i_caracter) == 'l' || Tipo_caracter(i_caracter) == 'd');
            if (Palabra_Reservada()) elemento += "\n";
            else
            {
                switch (i_caracter)
                {
                    case '.': Archivo_Libreria(); break;
                    case '(': elemento = "funcion\n"; break;
                    default: elemento = "identificador\n"; break;
                }
            }
        }
        private bool Comentario()
        {
            i_caracter = Leer.Read();
            switch (i_caracter)
            {
                case 47:
                    do
                    {
                        i_caracter = Leer.Read();
                    } while (i_caracter != 10);
                    return true;
                case 42:
                    do
                    {
                        do
                        {
                            i_caracter = Leer.Read();
                            if (i_caracter == 10) { Numero_linea++; }
                        } while (i_caracter != 42 && i_caracter != -1);
                        i_caracter = Leer.Read();
                    } while (i_caracter != 47 && i_caracter != -1);
                    if (i_caracter == -1) { Error(i_caracter); }
                    i_caracter = Leer.Read();
                    return true;
                default: return false;
            }
        }
        private void Numero_Real()
        {
            do
            {
                i_caracter = Leer.Read();
            } while (Tipo_caracter(i_caracter) == 'd');
            elemento = "numero_real\n";
        }
        private void Numero()
        {
            do
            {
                i_caracter = Leer.Read();
            } while (Tipo_caracter(i_caracter) == 'd');
            if ((char)i_caracter == '.') { Numero_Real(); }
            else
            {
                elemento = "numero_entero\n";
            }
        }
        ///////////////////Inicio del analisis léxico////////////////////////////
        /////////////////////////////////////////////////////////////////////////
        private void compilarSoluciónToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Rtbx_salida.Text = "Analizando...\n";
            Guardar(); elemento = "";
            N_error = 0; Numero_linea = 1;
            archivoback = archivo.Remove(archivo.Length - 1) + "back";
            Escribir = new StreamWriter(archivoback);
            Leer = new StreamReader(archivo);
            i_caracter = Leer.Read();
            do
            {
                elemento = "";
                switch (Tipo_caracter(i_caracter))
                {
                    case 'l': Identificador(); Escribir.Write(elemento); break;
                    case 'd': Numero(); Escribir.Write(elemento); break;
                    case 's': Simbolo(); Escribir.Write(elemento); i_caracter = Leer.Read(); break;
                    case '"': Cadena(); Escribir.Write("cadena\n"); i_caracter = Leer.Read(); break;
                    case 'c': Caracter(); Escribir.Write("caracter\n"); i_caracter = Leer.Read(); break;
                    case '/': if (Comentario()) { Escribir.Write("comentario\n"); } else { Escribir.Write("/\n"); } break;
                    case 'n': i_caracter = Leer.Read(); Numero_linea++; Escribir.Write("LF\n"); break;
                    case 'e': i_caracter = Leer.Read(); break;
                    default: Error(i_caracter); break;
                };

            } while (i_caracter != -1);
            Escribir.Write("Fin");
            Escribir.Close();
            Leer.Close();
            if (N_error == 0) { Rtbx_salida.AppendText("Errores Lexicos: " + N_error); A_Sintactico(); }
            else { Rtbx_salida.AppendText("Errores: " + N_error); }
        }

        //////////////////////////////////////////////////////////////////////////
        ////////////////////Funciones del análisis sintáctico///////////////////////////////////
        private void ErrorS(string e, string s)
        {
            Rtbx_salida.AppendText("Linea: " + Numero_linea + ". Error de sintaxis " + e + ", se esperaba " + s + "\n");
            token = ""; N_error++;
        }
        //----------------------------------------------------------------------------
        private void Include()
        {
            token = Leer.ReadLine();
            switch (token)
            {
                case "<":
                    token = Leer.ReadLine();
                    if (token == "Libreria")
                    {
                        token = Leer.ReadLine();
                        if (token == ">")
                        {
                            token = Leer.ReadLine();
                        }
                        else { ErrorS(token, ">"); N_error++; }
                    }
                    else { ErrorS(token, "nombre de archivo libreria"); N_error++; }
                    break;
                case "cadena": token = Leer.ReadLine(); break;
                //case "identificador": token = Leer.ReadLine(); break;
                default: ErrorS(token, "inclusión valida "); N_error++; break;
            }
        }
        //--------------------------------------------------------------------------
        private void Directriz()
        {
            token = Leer.ReadLine();
            switch (token)
            {
                case "include": Include(); break;
                case "define"://estructura para directriz #define 
                    break;
                case "if":    //estructura para directriz #if
                    break;
                case "error":    //estructura para directriz #error
                    break;
                // misma forma para las restantes directivas de procesador,
                default: ErrorS(token, "directriz de procesador"); break; ;
            }
        }
        //---------------------------------------------------------------------------
        private int Constante()
        {
            token = Leer.ReadLine();
            switch (token)
            {
                case "numero_real": return 1;
                case "numero_entero": return 1;
                case "caracter": return 1;
                case "identificador": return 1;
                default: return 0;
            }
        }
        //-----------------------------------------------------------------------------
        private void Bloque_Inicializacion()
        {
            do
            {
                token = Leer.ReadLine();
                if (token == "{")
                {
                    do
                    {
                        if (Constante() == 1) { token = "elemento"; }
                        switch (token)
                        {
                            case "elemento": token = Leer.ReadLine(); break;
                            case "{":
                                do
                                {
                                    if (Constante() == 0) { ErrorS(token, " inicializacion valida de arreglo."); }
                                    else { token = Leer.ReadLine(); }
                                } while (token == ",");
                                if (token == "}") { token = Leer.ReadLine(); }
                                else { ErrorS(token, "}"); }
                                break;
                        }
                    } while (token == ",");
                    if (token == "}") { token = Leer.ReadLine(); }
                    else { ErrorS(token, "}"); }
                }
                else { ErrorS(token, "{"); }
            } while (token == ",");
        }
        //-------------------------------------------------------------------------------
        private void D_Arreglos()
        {
            while (token == "[")
            {
                token = Leer.ReadLine();
                if (token == "identificador" || token == "numero_entero")
                {
                    token = Leer.ReadLine();
                    if (token == "]") { token = Leer.ReadLine(); }
                    else { ErrorS(token, "]"); }
                }
                else ErrorS(token, "valor de longitud");
            }
            switch (token)
            {
                case ";": token = Leer.ReadLine(); break;
                case "=":
                    token = Leer.ReadLine();
                    if (token == "{")
                    {
                        Bloque_Inicializacion();
                        if (token == "}")
                        {
                            token = Leer.ReadLine();
                            if (token == ";") { token = Leer.ReadLine(); }
                            else { ErrorS(token, ";"); }
                        }
                        else { ErrorS(token, "}"); }
                    }
                    else { ErrorS(token, "{"); }
                    break;
                default: ErrorS(token, "declaracion valida para arreglos."); break;
            }
        }
        //----------------------------------------------------------------------------
        // declaración de variables globales
        private void Dec_VGlobal() //se ha leido tipo e identificador
        {
            token = Leer.ReadLine();
            switch (token)
            {
                case "=":
                    if (Constante() == 1)
                    {
                        token = Leer.ReadLine();
                        if (token == ";") { token = Leer.ReadLine(); }
                        else { ErrorS(token, ";"); }
                    }
                    else { ErrorS(token, "inicializacion global valida"); }
                    break;
                case "[": D_Arreglos(); break;
                case ";": token = Leer.ReadLine(); break;
                default: ErrorS(token, ";"); break;
            }
        }
        //--------------------------------------------------------------------------
        // declaración general
        private void Declaracion()
        {
            switch (token)
            {
                case "dentificador": Dec_VGlobal(); break;
                case "funcion": Dec_Funcion(); break;
                default: ErrorS(token, "declaracion global valida"); break;
            }
        }
        private void Dec_Funcion() 
        {
            // Leer el tipo de retorno de la función (puede ser 'int', 'void', etc.)
            token = Leer.ReadLine();
            if (EsTipoRetorno(token)) // Verifica si es un tipo de retorno válido
            {
                string tipoRetorno = token;

                // Leer el nombre de la función (debe ser un identificador)
                token = Leer.ReadLine();
                if (Identificador(token))
                {
                    ErrorS(token, "Nombre de la función no válido");
                    return;
                }
                string nombreFuncion = token;

                // Leer el paréntesis de apertura '(' para los parámetros
                token = Leer.ReadLine();
                if (token != "(")
                {
                    ErrorS(token, "Se esperaba '(' para iniciar los parámetros de la función");
                    return;
                }

                // Procesar los parámetros de la función (si los hay)
                LeerParametros();

                // Leer el paréntesis de cierre ')'
                token = Leer.ReadLine();
                if (token != ")")
                {
                    ErrorS(token, "Se esperaba ')' para cerrar los parámetros de la función");
                    return;
                }

                // Leer la apertura del bloque de sentencias '{'
                token = Leer.ReadLine();
                if (token != "{")
                {
                    ErrorS(token, "Se esperaba '{' para iniciar el bloque de la función");
                    return;
                }

                // Procesar el bloque de sentencias de la función
                Bloque_Sentencia();

                // Leer la llave de cierre '}'
                token = Leer.ReadLine();
                if (token != "}")
                {
                    ErrorS(token, "Se esperaba '}' para cerrar el bloque de la función");
                }
            }
            else
            {
                ErrorS(token, "Tipo de retorno no válido");
            }

        }

        private bool Identificador(string token)
        {
            throw new NotImplementedException();
        }

        private bool EsTipoRetorno(string token)
        {
            throw new NotImplementedException();
        }

        private void LeerParametros()
        {
            throw new NotImplementedException();
        }

        private void F_Main() { }
        //-------------------------------------------------------------------------
        // Cabecera del código
        private int Cabecera()
        {
            token = Leer.ReadLine();
            do
            {
                if (P_Res_Tipo.IndexOf(token) >= 0)
                {
                    token = "tipo";
                }

                switch (token)
                {
                    case "#":
                        Directriz();
                        break;

                    case "tipo":
                        token = Leer.ReadLine();
                        if (token == "main") return 1;
                        else Declaracion();
                        break;

                    case "comentario":
                        token = Leer.ReadLine();
                        break;

                    case "typedef":
                        // Estructura typedef
                        break;

                    case "const":
                        // Estructura const
                        break;

                    case "extern":
                        // Estructura extern
                        break;

                    // Nueva validación para expresiones aritméticas
                    case "aritmetica":
                        if (EsExpresionAritmetica(token))
                        {
                            ProcesarExpresionAritmetica(token); // Método que puedes definir para manejar estas expresiones
                        }
                        token = Leer.ReadLine();
                        break;

                    // Nueva validación para expresiones lógicas
                    case "logica":
                        if (EsExpresionLogica(token))
                        {
                            ProcesarExpresionLogica(token); // Método que puedes definir para manejar estas expresiones
                        }
                        token = Leer.ReadLine();
                        break;

                    case "":
                        token = Leer.ReadLine();
                        break;

                    case "LF":
                        Numero_linea++;
                        token = Leer.ReadLine();
                        break;

                    default:
                        token = Leer.ReadLine();
                        break;
                }
            } while (token != "Fin" && token != "main");

            return 0;
        }
        private void ProcesarExpresionAritmetica(string expresion)
        {
            Console.WriteLine($"Procesando expresión aritmética: {expresion}");
            // Aquí puedes implementar el análisis o evaluación de la expresión.
        }

        private void ProcesarExpresionLogica(string expresion)
        {
            Console.WriteLine($"Procesando expresión lógica: {expresion}");
            // Aquí puedes implementar el análisis o evaluación de la expresión.
        }
        ////////////inicio del análisis sintáctico// // // // // //
        private void A_Sintactico()
        {
            Rtbx_salida.AppendText("\nAnalizando sintaxis...\n");
            N_error = 0; Numero_linea = 1;
            Leer = new StreamReader(archivoback);
            if (Cabecera() == 1)
            { f_Main(); }
            else { ErrorS(token, "funcion main()"); }
            Rtbx_salida.AppendText("Errores sintácticos: " + N_error);
            Leer.Close();
        }
        private void f_Main()
        {
            token = Leer.ReadLine();
            if (token == "(")
            {
                token = Leer.ReadLine();
                if (token == ")")
                {
                    token = Leer.ReadLine();
                    Bloque_Sentencia();

                }
                else Error(token, ")");
            }
            else Error(token, "(");
        }
        private void Bloque_Sentencia()
        {
            token = Leer.ReadLine();
            if (token == "{")
            {
                token = Leer.ReadLine();

                // Usamos un bucle para procesar múltiples sentencias dentro del bloque
                while (token != "}" && token != "fin")
                {
                    switch (token)
                    {
                        case "{":
                            Bloque_Sentencia();
                            break;
                        case "}":
                            return;  // Salimos del bloque al encontrar '}'
                        default:
                            Sentencias();
                            break;
                    }
                    // Leer el siguiente token para el próximo ciclo
                    token = Leer.ReadLine();
                }

                // Validamos si el bloque no fue cerrado correctamente
                if (token != "}")
                {
                    Error(token, "Se esperaba '}' para cerrar el bloque");
                }
            }
            else
            {
                Error(token, "{");  // Si el bloque no empieza con '{'
            }
        }


        private void Sentencias()
        {
            // selecciona la sentencia adecuada según el token
            switch (Token)
            {
                case "if": E_if(); break;
                case "comentario": token = Leer.ReadLine(); break;
                case "do": E_do(); break;
                case "while": E_while(); break;
                case "switch": E_switch(); break;
                case "identificador": Asignacion(); break;
                case "function": llamada_funcion(); break;
                default: Error(token, "Sentencia"); break;
            }
        }

       
        private void Error(string token, string mensaje)
        {
            Rtbx_salida.AppendText("Error de sintaxis: " + token + ", " + mensaje + "\n");
            N_error++;
        }
        private void llamada_funcion()
        {
            token = Leer.ReadLine();  // Leer el nombre de la función
            if (token == "identificador")  // Verifica si el token es un identificador válido
            {
                string nombreFuncion = token;
                token = Leer.ReadLine();  // Leer el paréntesis de apertura '('
                if (token == "(")
                {
                    // Analizar los parámetros de la función, si los hay
                    token = Leer.ReadLine();
                    while (token != ")" && token != ";")
                    {
                        // Aquí iría el análisis de cada parámetro de la función
                        // Puedes tener una función similar para los parámetros
                        token = Leer.ReadLine();
                    }
                    if (token == ")")
                    {
                        // Aquí se hace la llamada real a la función
                        Rtbx_salida.AppendText("Llamada a la función: " + nombreFuncion + "\n");
                    }
                    else
                    {
                        ErrorS(token, ")");
                    }
                }
                else
                {
                    ErrorS(token, "(");
                }
            }
            else
            {
                ErrorS(token, "identificador de función");
            }
        }
        //Se modifico la asignacion para poder leer un expresion aritmetica

        private void Asignacion()
        {
            // Leer el primer token (esperamos un identificador de variable)
            token = Leer.ReadLine();

            // Verificar que el token sea un identificador válido
            if (EsIdentificadorValido(token))
            {
                string variable = token; // Guardamos el nombre de la variable
                token = Leer.ReadLine(); // Leemos el siguiente token (esperamos un '=')

                if (token == "=")
                {
                    token = Leer.ReadLine(); // Leemos el valor que se asignará a la variable

                    // Verificar si es una expresión aritmética o lógica
                    if (EsExpresionAritmetica(token) || EsExpresionLogica(token))
                    {
                        // Si es válida, la asignación se registra en el control de salida
                        Rtbx_salida.AppendText($"Asignando expresión a {variable}: {token}\n");
                    }
                    else
                    {
                        // Si no es una expresión válida, se muestra un error
                        ErrorS(token, "Expresión inválida");
                    }
                }
                else
                {
                    // Si no encontramos el '=' esperados, mostrar un error
                    ErrorS(token, "=");
                }
            }
            else
            {
                // Si el primer token no es un identificador válido, mostrar un error
                ErrorS(token, "identificador");
            }
        }
        private bool EsIdentificadorValido(string token)
        {
            throw new NotImplementedException();
        }

        private void E_switch()
        {
            token = Leer.ReadLine();  // Leer la expresión del switch
            if (token == "(")
            {
                token = Leer.ReadLine();  // Leer la expresión a evaluar
                if (token == "expresion")  // Aquí deberías manejar la evaluación real de la expresión
                {
                    token = Leer.ReadLine();  // Leer el paréntesis de cierre ')'
                    if (token == ")")
                    {
                        token = Leer.ReadLine();  // Leer la apertura de las sentencias case
                        while (token != "default" && token != "}")
                        {
                            if (token == "case")
                            {
                                // Analiza el valor y el código a ejecutar
                                token = Leer.ReadLine();
                                if (token == "valor")  // Aquí procesas el valor de cada case
                                {
                                    token = Leer.ReadLine();  // Código asociado al case
                                }
                            }
                            token = Leer.ReadLine();
                        }
                        if (token == "default")
                        {
                            token = Leer.ReadLine();  // Código del caso por defecto
                        }
                        if (token == "}")
                        {
                            Rtbx_salida.AppendText("Estructura switch completada\n");
                        }
                        else
                        {
                            ErrorS(token, "}");
                        }
                    }
                    else
                    {
                        ErrorS(token, ")");
                    }
                }
                else
                {
                    ErrorS(token, "expresion");
                }
            }
        }

        private void E_while()
        {
            token = Leer.ReadLine();  // Leer la condición del while
            if (token == "(")
            {
                token = Leer.ReadLine();  // Leer la condición a evaluar
                if (token == "condicion")  // Aquí deberías manejar la condición real
                {
                    token = Leer.ReadLine();  // Leer el paréntesis de cierre ')'
                    if (token == ")")
                    {
                        token = Leer.ReadLine();  // Leer el código del bloque dentro del while
                        Rtbx_salida.AppendText("Estructura while completada\n");
                    }
                    else
                    {
                        ErrorS(token, ")");
                    }
                }
                else
                {
                    ErrorS(token, "condición");
                }
            }
            else
            {
                ErrorS(token, "(");
            }
        }

        private void E_do()
        {
            token = Leer.ReadLine();  // Leer el cuerpo del do
            if (token == "{")
            {
                token = Leer.ReadLine();  // Leer la condición de while
                if (token == "while")
                {
                    token = Leer.ReadLine();  // Leer la condición a evaluar
                    if (token == "(")
                    {
                        token = Leer.ReadLine();  // Evaluar la condición
                        if (token == "condicion")
                        {
                            token = Leer.ReadLine();  // Leer el paréntesis de cierre ')'
                            if (token == ")")
                            {
                                token = Leer.ReadLine();  // Leer el ';'
                                if (token == ";")
                                {
                                    Rtbx_salida.AppendText("Estructura do-while completada\n");
                                }
                                else
                                {
                                    ErrorS(token, ";");
                                }
                            }
                            else
                            {
                                ErrorS(token, ")");
                            }
                        }
                        else
                        {
                            ErrorS(token, "condición");
                        }
                    }
                    else
                    {
                        ErrorS(token, "(");
                    }
                }
                else
                {
                    ErrorS(token, "while");
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }
        
        private void E_if()
        {
            token = Leer.ReadLine();  // Leer la condición del if
            if (token == "(")
            {
                token = Leer.ReadLine();  // Leer la condición a evaluar
                if (token == "Bloque_Sentencia")  // Aquí deberías manejar la condición real
                {
                    token = Leer.ReadLine();  // Leer el paréntesis de cierre ')'
                    if (token == ")")
                    {
                        token = Leer.ReadLine();  // Leer el código dentro del if
                        if (token == "else")
                        {
                            token = Leer.ReadLine();  // Leer el código dentro del else
                            Rtbx_salida.AppendText("Estructura if-else completada\n");
                        }
                        else
                        {
                            Rtbx_salida.AppendText("Estructura if completada\n");
                        }
                    }
                    else
                    {
                        ErrorS(token, ")");
                    }
                }
                else
                {
                    ErrorS(token, "condición");
                }
            }
            else
            {
                ErrorS(token, "(");
            }
        }
        //Expresion aritmetica
        private bool EsExpresionAritmetica(string expresion)
        {
            // Validar una expresión aritmética simple
            
            string pattern = @"^\s*-?\d+(\.\d+)?\s*([\+\-\*\/\%]\s*-?\d+(\.\d+)?)*\s*$";

            return Regex.IsMatch(expresion, pattern);
        }
        //Expresion Logica
        private bool EsExpresionLogica(string expresion)
        {
            // Validar una expresión lógica
            
            string pattern = @"^\s*-?\d+(\.\d+)?\s*(==|!=|<|<=|>|>=)\s*-?\d+(\.\d+)?\s*$";

            return Regex.IsMatch(expresion, pattern);
        }
    }
}
    
