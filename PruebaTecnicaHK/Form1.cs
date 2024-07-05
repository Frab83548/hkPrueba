using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace PruebaTecnicaHK
{
    public partial class Form1 : Form
    {
        static string connectionString = System.Configuration.ConfigurationManager.AppSettings["CadenaBD"];

        public Form1()
        {
            InitializeComponent();
        }



        private void CargarAseguradosActivos()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string query = @"SELECT a.IdAsegurado, a.Dni, a.Nombre, a.FechaNacimiento, a.Estado
                             FROM Asegurados a
                             WHERE a.Estado = 'Activo';";

                    SqlCommand command = new SqlCommand(query, connection);
                    SqlDataAdapter adapter = new SqlDataAdapter(command);
                    DataTable dtAsegurados = new DataTable();
                    adapter.Fill(dtAsegurados);

                    // Agregar columna de Edad
                    dtAsegurados.Columns.Add("Edad", typeof(int));

                    foreach (DataRow row in dtAsegurados.Rows)
                    {
                        DateTime fechaNacimiento = Convert.ToDateTime(row["FechaNacimiento"]);
                        int edad = CalcularEdad(fechaNacimiento);
                        row["Edad"] = edad;
                    }

                    dgvAsegurados.DataSource = dtAsegurados;

                    // Configurar las columnas visibles en el DataGridView
                    dgvAsegurados.Columns["FechaNacimiento"].Visible = false; // Ocultar la columna de Fecha de Nacimiento

                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ocurrió un error al cargar los asegurados activos: " + ex.Message);
            }
        }

        private int CalcularEdad(DateTime fechaNacimiento)
        {
            DateTime fechaActual = DateTime.Today;
            int edad = fechaActual.Year - fechaNacimiento.Year;

            // Restar un año si aún no ha pasado el día del cumpleaños en el año actual
            if (fechaNacimiento > fechaActual.AddYears(-edad))
                edad--;

            return edad;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            CargarAseguradosActivos();
        }
        private void BuscarPoliza(int polizaId)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string query = @"SELECT p.IdPoliza, prod.Nombre AS Producto, tp.Nombre AS TipoProducto, p.Estado
                                     FROM Poliza p
                                     JOIN Producto prod ON p.IdProducto = prod.IdProducto
                                     JOIN TipoProducto tp ON p.IdTipoProducto = tp.IdTipoProducto
                                     WHERE p.IdPoliza = @IdPoliza;

                                     SELECT a.IdAsegurado, a.Dni, a.Nombre, a.FechaNacimiento, a.Estado
                                     FROM Asegurados a
                                     WHERE a.IdPoliza = @IdPoliza AND a.Estado = 'Activo';";

                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@IdPoliza", polizaId);

                    SqlDataAdapter adapter = new SqlDataAdapter(command);
                    DataSet ds = new DataSet();
                    adapter.Fill(ds);

                    if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                    {
                        DataRow poliza = ds.Tables[0].Rows[0];
                        txtProd.Text = poliza["Producto"].ToString();
                        txtTipoPro.Text = poliza["TipoProducto"].ToString();
                        txtEstado.Text = poliza["Estado"].ToString();

                        dgvAsegurados.DataSource = ds.Tables[1];
                    }
                    else
                    {
                        LimpiarControles();
                        MessageBox.Show("No se encontró ninguna póliza con el ID proporcionado.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ocurrió un error al buscar la póliza: " + ex.Message);
            }
        }

        private void EliminarAsegurado(int aseguradoId)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string query = "UPDATE Asegurados SET Estado = 'Inactivo' WHERE IdAsegurado = @IdAsegurado";
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@IdAsegurado", aseguradoId);

                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                }

                // Refrescar la grilla
                CargarAseguradosActivos();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ocurrió un error al eliminar el asegurado: " + ex.Message);
            }
        }

        private void LimpiarControles()
        {
            txtProd.Text = string.Empty;
            txtTipoPro.Text = string.Empty;
            txtEstado.Text = string.Empty;
            dgvAsegurados.DataSource = null;
        }

        private void btnBuscar_Click(object sender, EventArgs e)
        {
            int polizaId;
            if (int.TryParse(txtPolizaId.Text, out polizaId))
            {
                BuscarPoliza(polizaId);
            }
            else
            {
                MessageBox.Show("Por favor, ingrese un ID de póliza válido.");
            }
        }

        private void btnEliminar_Click_1(object sender, EventArgs e)
        {
            if (dgvAsegurados.SelectedRows.Count > 0)
            {
                int aseguradoId = Convert.ToInt32(dgvAsegurados.SelectedRows[0].Cells["IdAsegurado"].Value);
                EliminarAsegurado(aseguradoId);
            }
            else
            {
                MessageBox.Show("Por favor, seleccione un asegurado para eliminar.");
            }
        }
    }
}
