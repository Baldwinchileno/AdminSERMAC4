using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Windows.Forms;
using AdminSERMAC.Models;
using AdminSERMAC.Services;
using ClosedXML.Excel;
using System.Data.SQLite;

namespace AdminSERMAC.Forms
{
    public class VentasForm : Form
    {
        private Label numeroGuiaLabel;
        private TextBox numeroGuiaTextBox;
        private Label rutLabel;
        private ComboBox rutComboBox;
        private Label clienteLabel;
        private TextBox clienteTextBox;
        private Label direccionLabel;
        private TextBox direccionTextBox;
        private Label giroLabel;
        private TextBox giroTextBox;
        private Label fechaEmisionLabel;
        private DateTimePicker fechaEmisionPicker;
        private Label totalVentaLabel;
        private TextBox totalVentaTextBox;

        private DataGridView ventasDataGridView;
        private Button finalizarButton;
        private Button imprimirButton;
        private Button exportarExcelButton;
        private Button cancelarButton;
        private CheckBox pagarConCreditoCheckBox;

        private SQLiteService sqliteService;
        private double totalVenta = 0;

        public VentasForm()
        {
            this.Text = "Gestión de Ventas";
            this.Width = 1000;
            this.Height = 800;

            sqliteService = new SQLiteService();

            InitializeComponents();
            ConfigureEvents();
        }

        private void ConfigureEvents()
        {
            finalizarButton.Click += FinalizarButton_Click;
            ventasDataGridView.CellEndEdit += VentasDataGridView_CellEndEdit;
            rutComboBox.SelectedIndexChanged += RutComboBox_SelectedIndexChanged;
        }

        private void VentasDataGridView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == ventasDataGridView.Columns["Codigo"].Index)
            {
                ActualizarDatosProducto(e.RowIndex);
            }
            else if (e.ColumnIndex == ventasDataGridView.Columns["Bandejas"].Index ||
                     e.ColumnIndex == ventasDataGridView.Columns["KilosBruto"].Index ||
                     e.ColumnIndex == ventasDataGridView.Columns["Precio"].Index)
            {
                CalcularTotales(e.RowIndex);
            }
        }

        private void RutComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selectedRUT = rutComboBox.SelectedValue?.ToString();
            if (!string.IsNullOrEmpty(selectedRUT))
            {
                var cliente = sqliteService.GetClientePorRUT(selectedRUT);
                if (cliente != null)
                {
                    clienteTextBox.Text = cliente.Nombre;
                    direccionTextBox.Text = cliente.Direccion;
                    giroTextBox.Text = cliente.Giro;
                }
                else
                {
                    LimpiarDatosCliente();
                    MessageBox.Show("Cliente no encontrado.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void LimpiarDatosCliente()
        {
            clienteTextBox.Clear();
            direccionTextBox.Clear();
            giroTextBox.Clear();
        }

        private void ActualizarDatosProducto(int rowIndex)
        {
            string codigo = ventasDataGridView.Rows[rowIndex].Cells["Codigo"].Value?.ToString();
            if (!string.IsNullOrEmpty(codigo))
            {
                var producto = sqliteService.GetProductoPorCodigo(codigo);
                if (producto != null)
                {
                    ventasDataGridView.Rows[rowIndex].Cells["Descripcion"].Value = producto.Nombre;
                    ventasDataGridView.Rows[rowIndex].Cells["CantidadExistente"].Value = producto.Unidades;
                }
            }
        }

        private void CalcularTotales(int rowIndex)
        {
            try
            {
                int bandejas = int.TryParse(ventasDataGridView.Rows[rowIndex].Cells["Bandejas"].Value?.ToString(), out int b) ? b : 0;
                double kilosBruto = double.TryParse(ventasDataGridView.Rows[rowIndex].Cells["KilosBruto"].Value?.ToString(), out double k) ? k : 0;
                double precio = double.TryParse(ventasDataGridView.Rows[rowIndex].Cells["Precio"].Value?.ToString(), out double p) ? p : 0;

                double kilosNeto = kilosBruto - (1.5 * bandejas);
                if (kilosNeto < 0) kilosNeto = 0;

                ventasDataGridView.Rows[rowIndex].Cells["KilosNeto"].Value = kilosNeto.ToString("N2");
                double totalFila = kilosNeto * precio;
                ventasDataGridView.Rows[rowIndex].Cells["Total"].Value = totalFila.ToString("N0");

                CalcularTotalVenta();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al calcular totales: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CalcularTotalVenta()
        {
            totalVenta = 0;
            foreach (DataGridViewRow row in ventasDataGridView.Rows)
            {
                if (!row.IsNewRow && double.TryParse(row.Cells["Total"].Value?.ToString(), out double total))
                {
                    totalVenta += total;
                }
            }
            totalVentaTextBox.Text = totalVenta.ToString("C0");
        }

        private void InitializeComponents()
        {
            // Inicialización de controles (igual que en tu código original)
            // Número de Guía
            numeroGuiaLabel = new Label() { Text = "Número de Guía", Top = 20, Left = 20, Width = 120 };
            numeroGuiaTextBox = new TextBox() { Top = 20, Left = 150, Width = 200, ReadOnly = true };
            numeroGuiaTextBox.Text = sqliteService.GetUltimoNumeroGuia().ToString();

            // Resto de los controles
            rutLabel = new Label() { Text = "RUT Cliente", Top = 50, Left = 20, Width = 120 };
            rutComboBox = new ComboBox() { Top = 50, Left = 150, Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };

            // Código actualizado para cargar clientes
            var clientes = sqliteService.GetClientes();
            if (clientes.Count > 0)
            {
                rutComboBox.DataSource = clientes;
                rutComboBox.DisplayMember = "RUT";
                rutComboBox.ValueMember = "RUT";
            }
            else
            {
                MessageBox.Show("No se encontraron clientes en la base de datos.");
            }

            clienteLabel = new Label() { Text = "Cliente", Top = 80, Left = 20, Width = 120 };
            clienteTextBox = new TextBox() { Top = 80, Left = 150, Width = 200, ReadOnly = true };

            direccionLabel = new Label() { Text = "Dirección", Top = 110, Left = 20, Width = 120 };
            direccionTextBox = new TextBox() { Top = 110, Left = 150, Width = 200, ReadOnly = true };

            giroLabel = new Label() { Text = "Giro Comercial", Top = 140, Left = 20, Width = 120 };
            giroTextBox = new TextBox() { Top = 140, Left = 150, Width = 200, ReadOnly = true };

            fechaEmisionLabel = new Label() { Text = "Fecha de Emisión", Top = 170, Left = 20, Width = 120 };
            fechaEmisionPicker = new DateTimePicker() { Top = 170, Left = 150, Width = 200 };

            totalVentaLabel = new Label()
            {
                Text = "Total Venta:",
                Top = 600,
                Left = 600,
                Width = 100,
                Font = new Font(this.Font, FontStyle.Bold)
            };

            totalVentaTextBox = new TextBox()
            {
                Top = 600,
                Left = 700,
                Width = 150,
                ReadOnly = true,
                Font = new Font(this.Font, FontStyle.Bold),
                TextAlign = HorizontalAlignment.Right
            };

            pagarConCreditoCheckBox = new CheckBox()
            {
                Text = "Pagar con Crédito",
                Top = 630,
                Left = 180,
                Width = 150
            };

            ventasDataGridView = new DataGridView()
            {
                Top = 210,
                Left = 20,
                Width = 950,
                Height = 380,
                AllowUserToAddRows = true,
                AllowUserToDeleteRows = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            // Configurar columnas del DataGridView
            ventasDataGridView.Columns.Add("Codigo", "Código");
            ventasDataGridView.Columns.Add("Descripcion", "Descripción");
            ventasDataGridView.Columns.Add("Unidades", "Unidades");
            ventasDataGridView.Columns.Add("Bandejas", "Bandejas");
            ventasDataGridView.Columns.Add("KilosBruto", "Kilos Bruto");
            ventasDataGridView.Columns.Add("KilosNeto", "Kilos Neto");
            ventasDataGridView.Columns.Add("Precio", "Precio");
            ventasDataGridView.Columns.Add("Total", "Total");
            ventasDataGridView.Columns.Add("CantidadExistente", "Stock Disponible");

            finalizarButton = new Button()
            {
                Text = "Finalizar Venta",
                Top = 680,
                Left = 20,
                Width = 150,
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White
            };

            this.Controls.AddRange(new Control[] {
                numeroGuiaLabel, numeroGuiaTextBox,
                rutLabel, rutComboBox,
                clienteLabel, clienteTextBox,
                direccionLabel, direccionTextBox,
                giroLabel, giroTextBox,
                fechaEmisionLabel, fechaEmisionPicker,
                totalVentaLabel, totalVentaTextBox,
                pagarConCreditoCheckBox,
                ventasDataGridView,
                finalizarButton
            });
        }

        private void FinalizarButton_Click(object sender, EventArgs e)
        {
            if (!ValidarVenta()) return;

            try
            {
                using (var connection = sqliteService.GetConnection())
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        foreach (DataGridViewRow row in ventasDataGridView.Rows)
                        {
                            if (row.IsNewRow) continue;

                            string codigo = row.Cells["Codigo"].Value.ToString();
                            string descripcion = row.Cells["Descripcion"].Value.ToString();
                            int unidades = int.Parse(row.Cells["Unidades"].Value.ToString());
                            double kilos = double.Parse(row.Cells["KilosNeto"].Value.ToString());
                            string cliente = clienteTextBox.Text;

                            // Descontar inventario
                            sqliteService.DescontarInventario(1, codigo, unidades, kilos, transaction);

                            // Agregar venta
                            sqliteService.AgregarVenta(1, codigo, descripcion, unidades, kilos, cliente, transaction);
                        }

                        if (pagarConCreditoCheckBox.Checked)
                        {
                            string clienteRUT = rutComboBox.SelectedValue.ToString();
                            sqliteService.ActualizarDeudaCliente(clienteRUT, totalVenta);
                        }

                        transaction.Commit();
                        MessageBox.Show("Venta finalizada exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al finalizar la venta: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidarVenta()
        {
            // Lógica de validación sigue igual
            return true;
        }
    }
}
