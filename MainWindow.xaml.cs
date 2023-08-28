using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp1
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            dataBase.strcon();
            dt_cart_create();
            Load_Clients();
            Load_Products();
        }

        public static double costcount;        // Общая сумма
        int a;
        public static bool show_order = false;
        bool search_client = false;         // Поиск клиента
        bool search_prod = false;           // Поиск товара
        bool check_client = false;          // Добавление/редактирование клиентов
        bool check_categ = false;           // Добавление/редактирование категории
        bool check_prod = false;            // Добавление/редактирование товара
        string categ;
        string title;
        DataBase dataBase = new DataBase();
        public static DataTable dt_cart = new DataTable();
        public static DataTable dt_order = new DataTable();
        DataTable dt;

        Product product = new Product();
        Cart cart = new Cart();
        Clients client = new Clients();
        Orders orders = new Orders();


        #region Клиенты

        // Загрузка клиентов:
        void Load_Clients()
        {

            dataGrid1.Items.Clear();
            DataBase.sqlcmd = @"SELECT * 
                                FROM `clients`
                                ORDER BY ФИО";
            DataBase.dt_clients = dataBase.Connect(DataBase.sqlcmd);
            for (int i = 0; i < DataBase.dt_clients.Rows.Count; i++)
            {
                string date = DataBase.dt_clients.Rows[i][4].ToString();
                date = date.Remove(date.Length - 8);
                Clients data = new Clients()
                {
                    Client_id = Convert.ToString(DataBase.dt_clients.Rows[i][0]),
                    FIO = Convert.ToString(DataBase.dt_clients.Rows[i][1]),
                    Phone = Convert.ToString(DataBase.dt_clients.Rows[i][2]),
                    Email = Convert.ToString(DataBase.dt_clients.Rows[i][3]),
                    BDay = date
                };
                dataGrid1.Items.Add(data);
            }

        }

        // Поиск клиентов:
        private void button_clients_search_Click(object sender, RoutedEventArgs e)
        {
            Clients data = new Clients();
            if (search_client == false)
            {
                data.FIO = textBox.Text;
                data.Phone = textBox1.Text;
                data.Email = textBox2.Text;
                try
                {
                    data.BDay = datePicker1.SelectedDate.Value.ToString("yyyy-MM-dd");
                }
                catch { }

                DataBase.sqlcmd = $@"SELECT * 
                                 FROM clients
                                 WHERE `Номер телефона` = '{data.Phone}' 
                                    OR `Email` = '{data.Email}'
                                    OR `Дата рождения` = '{data.BDay}'"
                ;

                if (data.FIO != "")
                {
                    DataBase.sqlcmd += $@" OR `ФИО` LIKE '%{data.FIO}%'";
                }
                DataBase.sqlcmd += $"\nORDER BY ФИО";

                button_clients_search.Content = "Сбросить";
                search_client = true;
            }
            else
            {
                DataBase.sqlcmd = @"SELECT * 
                                    FROM `clients`
                                    ORDER BY ФИО";
                button_clients_search.Content = "Найти";
                search_client = false;
                textBox.Text = "";
                textBox1.Text = "";
                textBox2.Text = "";
                datePicker1.Text = "";
            }

            DataBase.dt_clients = dataBase.Connect(DataBase.sqlcmd);
            dataGrid1.Items.Clear();


            for (int i = 0; i < DataBase.dt_clients.Rows.Count; i++)
            {
                string date = DataBase.dt_clients.Rows[i][4].ToString();
                date = date.Remove(date.Length - 8);

                data = new Clients()
                {
                    Client_id = Convert.ToString(DataBase.dt_clients.Rows[i][0]),
                    FIO = Convert.ToString(DataBase.dt_clients.Rows[i][1]),
                    Phone = Convert.ToString(DataBase.dt_clients.Rows[i][2]),
                    Email = Convert.ToString(DataBase.dt_clients.Rows[i][3]),
                    BDay = date
                };
                dataGrid1.Items.Add(data);
            }

            label1.Content = "";
            dataGrid_orders.Items.Clear();

        }

        // Изменение параметров поиска клиентов:
        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            button_clients_search.Content = "Найти";
            search_client = false;
        }
        private void textBox1_TextChanged(object sender, TextChangedEventArgs e)
        {
            button_clients_search.Content = "Найти";
            search_client = false;
        }
        private void textBox2_TextChanged(object sender, TextChangedEventArgs e)
        {
            button_clients_search.Content = "Найти";
            search_client = false;
        }
        private void datePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            button_clients_search.Content = "Найти";
            search_client = false;
        }

        // Выбор клиента и просмотр его заказов:
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            dataGrid_orders.Items.Clear();
            client = dataGrid1.SelectedItem as Clients;
            label1.Content = $"ID:   {client.Client_id} \n" +
                             $"ФИО:   {client.FIO} \n" +
                             $"Номер телефона:   {client.Phone} \n" +
                             $"Email:   {client.Email} \n" +
                             $"День рождения:   {client.BDay}";

            /*DataBase.sqlcmd = $@"SELECT *
                                 FROM `order`
                                 WHERE client_id = {client.Client_id}";*/
            DataBase.sqlcmd = $@"SELECT `order`.`order_id`, `order`.`client_id`, `order`.`Дата`, `order`.`Общая сумма`, `order_status`.`Статус заказа`
                                 FROM `order` 
	                             LEFT JOIN `order_status` ON `order`.`Статус заказа` = `order_status`.`status_id`
                                 WHERE client_id = {client.Client_id}
                                 ORDER BY `order`.`order_id`; ";
            DataBase.dt_clients = dataBase.Connect(DataBase.sqlcmd);

            for (int i = 0; i < DataBase.dt_clients.Rows.Count; i++)
            {
                orders = new Orders()
                {
                    Order_id = Convert.ToString(DataBase.dt_clients.Rows[i][0]),
                    Client_id = Convert.ToString(DataBase.dt_clients.Rows[i][1]),
                    Order_Date = Convert.ToString(DataBase.dt_clients.Rows[i][2]),
                    Total_cost = Convert.ToString(DataBase.dt_clients.Rows[i][3]),
                    Order_status = Convert.ToString(DataBase.dt_clients.Rows[i][4])
                };
                dataGrid_orders.Items.Add(orders);
            }
        }

        // Просмотр заказа клиента:
        private void TextBlock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            show_order = true;

            Order order = new Order();

            order.label.Content = $"Заказ № {DataBase.dt_clients.Rows[dataGrid_orders.SelectedIndex][0]}";
            order.label_FIO.Content = client.FIO;
            order.label_Phone.Content = client.Phone;
            order.label_Email.Content = client.Email;
            order.label_totalCost.Content = DataBase.dt_clients.Rows[dataGrid_orders.SelectedIndex][3];
            order.label_OrderStatus.Content = DataBase.dt_clients.Rows[dataGrid_orders.SelectedIndex][4].ToString();
            // Какая-то ошибка:
            order.label_Date.Content = DateTime.Parse(DataBase.dt_clients.Rows[dataGrid_orders.SelectedIndex][2].ToString()).ToString("dd/MM/yyyy  HH:mm:ss");

            order.Grid_client.Visibility = Visibility.Hidden;
            order.comboBox1.Visibility = Visibility.Visible;
            order.button.Visibility = Visibility.Hidden;
            order.button2.Visibility= Visibility.Visible;

            DataBase.sqlcmd = $@"SELECT cart.cart_id, product.Наименование, cart.Количество, product.Цена
                                FROM product INNER JOIN (`order` INNER JOIN cart ON order.order_id = cart.order_id) ON product.product_id = cart.product_id
                                WHERE cart.order_id = {DataBase.dt_clients.Rows[dataGrid_orders.SelectedIndex][0]}"
            ;

            dt_order = new DataTable();


            dt_order = dataBase.Connect(DataBase.sqlcmd);
            order.add_cart();
            order.ShowDialog();

            show_order = false;
        }

        //Создание, редактирование клиента
        private void buttonAddChange_Click(object sender, RoutedEventArgs e)
        {
            if (check_client == false)
            {
                string error = "Введите: ";
                if (textBox3.Text == "")
                {
                    error += "ФИО, ";
                }
                if (textBox4.Text == "")
                {
                    error += "Номер телефона, ";
                }
                if (textBox5.Text == "")
                {
                    error += "Email, ";
                }
                if (datePicker2.SelectedDate == null)
                {
                    error += "Дату рождения, ";
                }

                error = error.Remove(error.Length - 2);

                if (textBox3.Text == "" || textBox4.Text == "" || textBox5.Text == "" || datePicker2.SelectedDate == null)
                {
                    MessageBox.Show(error);
                }
                else
                {
                    Clients data = new Clients();
                    data.FIO = textBox3.Text;
                    data.Phone = textBox4.Text;
                    data.Email = textBox5.Text;
                    data.BDay = "";
                    try
                    {
                        data.BDay = datePicker2.SelectedDate.Value.ToString("yyyy-MM-dd");
                    }
                    catch { }
                    DataBase.sqlcmd = $@"SELECT EXISTS 
                                     (SELECT * 
                                      FROM clients
                                      WHERE ФИО = '{data.FIO}' AND (`Номер телефона` = '{data.Phone}' OR Email = '{data.Email}'))"
                    ;
                    DataBase.dt_clients = dataBase.Connect(DataBase.sqlcmd);

                    if (DataBase.dt_clients.Rows[0][0].ToString() == "1")
                    {
                        MessageBox.Show("Данный клиент уже зарегистрирован");
                    }
                    else
                    {
                        DataBase.sqlcmd = $@"START TRANSACTION;

                                         INSERT INTO clients
                                                (ФИО, `Номер телефона`, Email, `Дата рождения`)
                                         VALUES ('{data.FIO}', '{data.Phone}', '{data.Email}', '{data.BDay}');

                                         SELECT EXISTS 
                                         (SELECT * 
                                          FROM clients
                                          WHERE ФИО = '{data.FIO}' AND `Номер телефона` = '{data.Phone}' AND Email = '{data.Email}');

                                          COMMIT;"
                        ;
                        DataBase.dt_clients = dataBase.Connect(DataBase.sqlcmd);
                        if (DataBase.dt_clients.Rows[0][0].ToString() == "1")
                        {
                            Load_Clients();
                            MessageBox.Show("Клиент успешно зарегистрирован!");
                            textBox3.Text = "";
                            textBox4.Text = "";
                            textBox5.Text = "";
                            datePicker2.Text = "";
                        }
                        else
                        {
                            MessageBox.Show("Ошибка регистрации");
                        }

                    }
                }
            }
            else
            {
                string a = "";
                try
                {
                    a = datePicker2.SelectedDate.Value.ToString("yyyy-MM-dd");
                }
                catch { }
                DataBase.sqlcmd = $@"UPDATE clients
                                     SET ФИО = '{textBox3.Text}',
                                         `Номер телефона` = '{textBox4.Text}',
                                         Email = '{textBox5.Text}',
                                         `Дата рождения` = '{a}'
                                     WHERE Client_id = {client.Client_id}"
                ;
                DataBase.dt_clients = dataBase.Connect(DataBase.sqlcmd);
                Load_Clients();

                MessageBox.Show("Данные клиента успешно изменены!");
                textBox3.Text = "";
                textBox4.Text = "";
                textBox5.Text = "";
                datePicker2.Text = "";
                check_client = false;
                buttonAddChange.Content = "Создать";
            }

        }

        // Меню.Редактировать клиента:
        private void MenuItemChange_Click(object sender, RoutedEventArgs e)
        {
            client = dataGrid1.SelectedItem as Clients;
            textBox3.Text = client.FIO;
            textBox4.Text = client.Phone;
            textBox5.Text = client.Email;
            datePicker2.Text = client.BDay;

            buttonAddChange.Content = "Изменить";
            check_client = true;
        }
        // Меню.Удалить клиента:
        private void MenuItemDelete_Click(object sender, RoutedEventArgs e)
        {
            client = dataGrid1.SelectedItem as Clients;
            DataBase.sqlcmd = $@"DELETE FROM `clients`
                                WHERE client_id = {client.Client_id}"
            ;

            string messageBoxText = "Вы действительно хотите удалить запись?";
            string caption = "";
            MessageBoxButton button = MessageBoxButton.YesNo;
            MessageBoxResult result;
            result = MessageBox.Show(messageBoxText, caption, button, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                dataBase.Connect(DataBase.sqlcmd);
                Load_Clients();
                MessageBox.Show("Запись успешно удалена");

                label1.Content = "";
                dataGrid_orders.Items.Clear();
            }

        }

        // Ограничения для ввода номера:
        private void textBox1_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (textBox1.Text.Length > 10 || !Char.IsDigit(e.Text, 0))
            {
                e.Handled = true;
            }
        }
        private void textBox1_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true;
            }
        }
        private void textBox4_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (textBox4.Text.Length > 10 || !Char.IsDigit(e.Text, 0))
            {
                e.Handled = true;
            }
        }
        private void textBox4_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true;
            }
        }
        #endregion Клиенты

        #region Склад

        // Загрузка товаров:
        void Load_Products()
        {
            dataGrid2.Items.Clear();
            treeView.Items.Clear();

            TreeViewItem treeNode = new TreeViewItem();

            DataBase.sqlcmd = @"SELECT product.product_id, category.Категория, product.Наименование, product.`На складе`, product.Цена
                                FROM category INNER JOIN product ON category.category_id = product.Категория
                                ORDER BY category.Категория;";
            DataBase.dt_clients = dataBase.Connect(DataBase.sqlcmd);

            for (int i = 0; i < DataBase.dt_clients.Rows.Count; i++)
            {
                Product product = new Product()
                {
                    Product_id = Convert.ToString(DataBase.dt_clients.Rows[i][0]),
                    Category = Convert.ToString(DataBase.dt_clients.Rows[i][1]),
                    Title = Convert.ToString(DataBase.dt_clients.Rows[i][2]),
                    Count = Convert.ToString(DataBase.dt_clients.Rows[i][3]),
                    Cost = Convert.ToString(DataBase.dt_clients.Rows[i][4]),
                };
                dataGrid2.Items.Add(product);

                if (i == 0)
                {
                    treeNode = new TreeViewItem();
                    treeNode.Header = product.Category;
                    treeNode.Items.Add($"{product.Title}   |   {product.Count} шт.   |   {product.Cost} руб.");

                    if (i != DataBase.dt_clients.Rows.Count - 1)
                    {
                        if (Convert.ToString(DataBase.dt_clients.Rows[i][1]) != Convert.ToString(DataBase.dt_clients.Rows[i + 1][1]))
                        {

                            treeView.Items.Add(treeNode);

                            treeNode = new TreeViewItem();
                        }
                    }
                    else
                    {
                        treeView.Items.Add(treeNode);

                        treeNode = new TreeViewItem();
                    }
                    //((TreeViewItem)((TreeViewItem)treeView.Items[a]).Items[b]).Items.Add(new TreeViewItem() { Header = product.Title });
                }
                else
                {
                    if (i != DataBase.dt_clients.Rows.Count - 1)
                    {
                        if (Convert.ToString(DataBase.dt_clients.Rows[i][1]) == Convert.ToString(DataBase.dt_clients.Rows[i + 1][1]))
                        {
                            treeNode.Items.Add($"{product.Title}   |   {product.Count} шт.   |   {product.Cost} руб.");
                        }
                        else
                        {
                            treeNode.Header = product.Category;
                            treeNode.Items.Add($"{product.Title}   |   {product.Count} шт.   |   {product.Cost} руб.");
                            treeView.Items.Add(treeNode);


                            treeNode = new TreeViewItem();
                        }
                    }
                    //((TreeViewItem)((TreeViewItem)treeView.Items[a]).Items[b]).Items.Add(new TreeViewItem() { Header = product.Title });
                    else
                    {
                        treeNode.Header = product.Category;
                        treeNode.Items.Add($"{product.Title}   |   {product.Count} шт.   |   {product.Cost} руб.");
                        treeView.Items.Add(treeNode);


                        treeNode = new TreeViewItem();
                    }
                }
            }

            DataBase.sqlcmd = "SELECT * FROM category ORDER BY Категория";
            dt = dataBase.Connect(DataBase.sqlcmd);
            comboBox_category.Items.Clear();
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                for (int j = 0; j < DataBase.dt_clients.Rows.Count; j++)
                {
                    if (dt.Rows[i][1].ToString() == DataBase.dt_clients.Rows[j][1].ToString())
                    {
                        break;
                    }
                    else
                    {
                        if (dt.Rows[i][1].ToString() != DataBase.dt_clients.Rows[j][1].ToString())
                        {
                            if (j == DataBase.dt_clients.Rows.Count - 1)
                            {
                                treeView.Items.Add(dt.Rows[i][1]);

                            }
                        }
                    }
                }
                comboBox_category.Items.Add(dt.Rows[i][1]);
            }
        }

        // Добавить/изменить категорию:
        private void button2_Click(object sender, RoutedEventArgs e)
        {
            if (check_categ == false)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (dt.Rows[i][1].ToString().Contains(textBox_addCategory.Text))
                    {
                        MessageBox.Show("Данная категория уже существует");
                        break;
                    }
                    else if (i == dt.Rows.Count - 1)
                    {
                        if (textBox_addCategory.Text != dt.Rows[i][1].ToString())
                        {
                            DataBase.sqlcmd = $@"START TRANSACTION;

                                                 INSERT INTO category (Категория) VALUES('{textBox_addCategory.Text}')
                                                 WHERE NOT EXISTS (SELECT 1 FROM category WHERE (Категория) IN ('{textBox_addCategory.Text}'));

                                                 SELECT * FROM category WHERE Категория = '{textBox_addCategory.Text}';

                                                 COMMIT;"
                            ;
                            DataTable dtt = dataBase.Connect(DataBase.sqlcmd);
                            if (dtt.Rows.Count > 0)
                            {
                                Load_Products();
                                textBox_addCategory.Text = "";
                                MessageBox.Show("Категория успешно добавлена");
                            }
                            else
                            {
                                MessageBox.Show("Ошибка");
                            }
                            break;
                        }
                    }
                }
            }
            else
            {
                DataBase.sqlcmd = $@"START TRANSACTION;

                                     UPDATE category 
                                     SET Категория = '{textBox_addCategory.Text}'
                                     WHERE Категория = '{categ}';

                                     SELECT * FROM category WHERE Категория = '{textBox_addCategory.Text}';

                                     COMMIT;"
                ;
                DataTable dtt = dataBase.Connect(DataBase.sqlcmd);
                if (dtt.Rows.Count > 0)
                {
                    Load_Products();
                    textBox_addCategory.Text = "";
                    button2.Content = "Добавить категорию";
                    MessageBox.Show("Категория успешно изменена");
                }
                else
                {
                    MessageBox.Show("Ошибка");
                }
                check_categ = false;
            }
        }

        // Добавить/изменить товар:
        private void button3_Click(object sender, RoutedEventArgs e)
        {
            if (check_prod == false)
            {
                DataBase.sqlcmd = $@"INSERT INTO a0686088_test.`product` (Категория, Наименование, `На складе`, Цена)
                                 SELECT a0686088_test.category.category_id, '{textBox_addTitle.Text}', {textBox_addCount.Text}, {textBox_addCost.Text}
                                 FROM a0686088_test.product, a0686088_test.category
                                 WHERE a0686088_test.category.Категория='{comboBox_category.SelectedItem}' 
                                       AND NOT EXISTS (SELECT 1 FROM a0686088_test.`product` WHERE (Наименование) IN ('{textBox_addTitle.Text}')) 
                                 LIMIT 1;"
                ;
                dataBase.Connect(DataBase.sqlcmd);
                Load_Products();
                textBox_addTitle.Text = "";
                textBox_addCount.Text = "";
                textBox_addCost.Text = "";
            }
            else
            {
                DataBase.sqlcmd = $@"UPDATE `product`, `category`
                                     SET `product`.`Категория` = category.category_id, 
                                         `product`.`Наименование` = '{textBox_addTitle.Text}', 
                                         `На складе` = '{textBox_addCount.Text}', 
                                         `Цена` = '{textBox_addCost.Text}' 
                                     WHERE `product`.`Наименование` = '{title}' AND `category`.`Категория` = '{comboBox_category.SelectedItem}'"
                ;
                dataBase.Connect(DataBase.sqlcmd);
                Load_Products();
                textBox_addTitle.Text = "";
                textBox_addCount.Text = "";
                textBox_addCost.Text = "";

                check_prod = false;
                button3.Content = "Добавить товар";
            }
        }

        // Выбор товара/категории при открытии меню:
        private void treeView_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            TreeViewItem treeViewItem = VisualUpwardSearch(e.OriginalSource as DependencyObject);

            if (treeViewItem != null)
            {
                treeViewItem.Focus();
                e.Handled = true;
            }
        }
        static TreeViewItem VisualUpwardSearch(DependencyObject source)
        {
            while (source != null && !(source is TreeViewItem))
            {
                source = VisualTreeHelper.GetParent(source);
            }

            return source as TreeViewItem;
        }

        // Меню.Изменить товар/категорию:
        private void ItemChange_Click(object sender, RoutedEventArgs e)
        {

            // выбор категории:
            try
            {
                textBox_addCategory.Text = ((TreeViewItem)treeView.SelectedItem).Header.ToString();
                categ = textBox_addCategory.Text;

                check_categ = true;
                button2.Content = "Изменить";
            }
            // выбор товара:
            catch
            {
                try
                {
                    string[] words = treeView.SelectedItem.ToString().Split(new string[] { "   |   ", " шт.   |   ", " руб." }, StringSplitOptions.RemoveEmptyEntries);
                    textBox_addCount.Text = words[1];

                    if (words.Length == 3)
                    {
                        DataBase.sqlcmd = $@"SELECT `product`.`product_id`, `category`.`Категория`, `product`.`Наименование`, `product`.`На складе`, `product`.`Цена`
                                         FROM `product` 
	                                     LEFT JOIN `category` ON `product`.`Категория` = `category`.`category_id`
                                         WHERE product.Наименование = '{words[0]}'";
                        dt = dataBase.Connect(DataBase.sqlcmd);

                        comboBox_category.SelectedItem = dt.Rows[0][1].ToString();
                        textBox_addTitle.Text = dt.Rows[0][2].ToString();
                        textBox_addCount.Text = dt.Rows[0][3].ToString();
                        textBox_addCost.Text = dt.Rows[0][4].ToString();

                        categ = dt.Rows[0][1].ToString();
                        title = dt.Rows[0][2].ToString();

                        check_prod = true;
                        button3.Content = "Изменить";
                    }
                }
                catch
                {
                    textBox_addCategory.Text = treeView.SelectedItem.ToString();
                    categ = textBox_addCategory.Text;

                    check_categ = true;
                    button2.Content = "Изменить";
                }
            }
        }
        // Меню.Удалить товар/категорию:
        private void ItemDelete_Click(object sender, RoutedEventArgs e)
        {
            string del;
            string a;
            // выбор категории:
            try
            {
                del = ((TreeViewItem)treeView.SelectedItem).Header.ToString();
                a = " эту категорию";
                DataBase.sqlcmd = $@"DELETE FROM category
                                     WHERE Категория = '{del}'";
            }
            // выбор товара:
            catch
            {
                try
                {
                    string[] words = treeView.SelectedItem.ToString().Split(new string[] { "   |   ", " шт.   |   ", " руб." }, StringSplitOptions.RemoveEmptyEntries);
                    del = words[2];
                    del = words[0];
                    a = "этот товар";
                    DataBase.sqlcmd = $@"DELETE FROM product
                                         WHERE Наименование = '{del}'";
                }
                catch
                {
                    del = treeView.SelectedItem.ToString();
                    a = "эту категорию";
                    DataBase.sqlcmd = $@"DELETE FROM category
                                         WHERE Категория = '{del}'";
                }
            }

            string messageBoxText = $"Вы действительно хотите удалить {a}?";
            string caption = "";
            MessageBoxButton button = MessageBoxButton.YesNo;
            MessageBoxResult result;
            result = MessageBox.Show(messageBoxText, caption, button, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                dataBase.Connect(DataBase.sqlcmd);
                Load_Products();
            }
        }

        // Ограничения ввода количества:
        private void textBox_addCount_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (textBox_addCount.Text.Length > 5 || !Char.IsDigit(e.Text, 0))
            {
                e.Handled = true;
            }
        }
        private void textBox_addCount_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true;
            }
        }
        // Ограничения ввода стоимости:
        private void textBox_addCost_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (textBox_addCost.Text.Length > 8 || !Char.IsDigit(e.Text, 0))
            {
                e.Handled = true;
            }
        }
        private void textBox_addCost_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true;
            }
        }

        #endregion Склад

        #region Корзина

        // Создание таблицы для корзины:
        private void dt_cart_create()
        {
            dt_cart = new DataTable();

            dt_cart.Columns.Add("Product_id", typeof(int));
            dt_cart.Columns.Add("Title", typeof(string));
            dt_cart.Columns.Add("Count_inCart", typeof(int));
            dt_cart.Columns.Add("Cost_inCart", typeof(double));
            dt_cart.Columns.Add("maxCount", typeof(int));
            dt_cart.Columns.Add("Prod_Cost", typeof(double));
        }

        // Добавление в корзину:
        private void button_Cart_Click(object sender, RoutedEventArgs e)
        {
            chip_count.Visibility = Visibility.Visible;
            int b = 0;

            product = dataGrid2.SelectedItem as Product;
            cart = new Cart()
            {
                Count_inCart = "0",
                maxCount = product.Count
            };

            if (dt_cart.Rows.Count == 0)
            {
                dt_cart.Rows.Add(product.Product_id, product.Title, cart.Count_inCart+1, product.Cost, cart.maxCount, product.Cost);
            }
            else
            {
                for (int i = 0; i < dt_cart.Rows.Count; i++)
                {
                    if (dt_cart.Rows[i][0].ToString() == product.Product_id)
                    {
                        if (int.Parse(dt_cart.Rows[i][2].ToString()) < int.Parse(cart.maxCount))
                        {
                            dt_cart.Rows[i][2] = int.Parse(dt_cart.Rows[i][2].ToString()) + 1;
                            dt_cart.Rows[i][3] = double.Parse(dt_cart.Rows[i][2].ToString()) * double.Parse(dt_cart.Rows[i][5].ToString());
                        }
                        break;
                    }
                    else if (i == dt_cart.Rows.Count - 1)
                    {
                        dt_cart.Rows.Add(product.Product_id, product.Title, cart.Count_inCart, product.Cost, cart.maxCount, product.Cost);
                    }
                }
            }

            costcount = 0;
            for (int i = 0; i < dt_cart.Rows.Count; i++)
            {
                costcount += double.Parse(dt_cart.Rows[i][3].ToString());
                b += int.Parse(dt_cart.Rows[i][2].ToString());
            }

            dataGrid_cart.ItemsSource = dt_cart.DefaultView;
            chip_count.Content = b.ToString();
            label3.Content = costcount;

        }

        // Увеличение количества товара в корзине:
        private void buttonUp_Click(object sender, RoutedEventArgs e)
        {
            chip_count.Visibility = Visibility.Visible;
            int b = 0;
            int a = dataGrid_cart.SelectedIndex;

            if (int.Parse(dt_cart.Rows[a][2].ToString()) < int.Parse(dt_cart.Rows[a][4].ToString()))
            {
                dt_cart.Rows[a][2] = int.Parse(dt_cart.Rows[a][2].ToString()) + 1;
            }
            costcount = 0;
            for (int i = 0; i < dt_cart.Rows.Count; i++)
            {
                dt_cart.Rows[i][3] = double.Parse(dt_cart.Rows[i][2].ToString()) * double.Parse(dt_cart.Rows[i][5].ToString());
                costcount += double.Parse(dt_cart.Rows[i][3].ToString());
                b += int.Parse(dt_cart.Rows[i][2].ToString());
            }
            label3.Content = costcount;
            chip_count.Content = b.ToString();
            dataGrid_cart.ItemsSource = dt_cart.DefaultView;
        }

        // Уменьшение товара в корзине:
        private void buttonDown_Click(object sender, RoutedEventArgs e)
        {
            chip_count.Visibility = Visibility.Visible;
            int b = 0;
            a = dataGrid_cart.SelectedIndex;

            if (int.Parse(dt_cart.Rows[a][2].ToString()) > 0)
            {
                dt_cart.Rows[a][2] = int.Parse(dt_cart.Rows[a][2].ToString()) - 1;
            }
            if(int.Parse(dt_cart.Rows[a][2].ToString()) == 0)
            {
                dt_cart.Rows[a].Delete();
            }
            costcount = 0;
            for (int i = 0; i < dt_cart.Rows.Count; i++)
            {
                dt_cart.Rows[i][3] = int.Parse(dt_cart.Rows[i][2].ToString()) * int.Parse(dt_cart.Rows[i][5].ToString());
                costcount += int.Parse(dt_cart.Rows[i][3].ToString());
                b += int.Parse(dt_cart.Rows[i][2].ToString());
            }
            if (b == 0)
            {
                chip_count.Visibility = Visibility.Hidden;
            }
            label3.Content = costcount; 
            chip_count.Content = b.ToString();
            dataGrid_cart.ItemsSource = dt_cart.DefaultView;
        }

        // Удаление товара из корзины:
        private void delete_fromCart(object sender, RoutedEventArgs e)
        {
            a = dataGrid_cart.SelectedIndex;
            chip_count.Visibility = Visibility.Visible;
            int b = 0;
            
            dt_cart.Rows[a].Delete();
            costcount = 0;
            for (int i = 0; i < dt_cart.Rows.Count; i++)
            {
                dt_cart.Rows[i][3] = double.Parse(dt_cart.Rows[i][2].ToString()) * double.Parse(dt_cart.Rows[i][5].ToString());
                costcount += double.Parse(dt_cart.Rows[i][3].ToString());
                b += int.Parse(dt_cart.Rows[i][2].ToString());
            }
            if (b == 0)
            {
                chip_count.Visibility = Visibility.Hidden;
            }
            label3.Content = costcount;
            chip_count.Content = b.ToString();
            dataGrid_cart.ItemsSource = dt_cart.DefaultView;
        }

        // Сворачинание/разворачивание корзины:
        private void PackIcon_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            expander.IsExpanded = expander.IsExpanded == false;
        }

        #endregion Корзина

        #region Заказ

        // Поиск товаров:
        private void button1_Click_1(object sender, RoutedEventArgs e)
        {
            if (search_prod == false && textBox_prodSearch.Text != "")
            {
                string[] words = textBox_prodSearch.Text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                DataBase.sqlcmd = @"SELECT `product`.`product_id`, `category`.`Категория`, `product`.`Наименование`, `product`.`На складе`, `product`.`Цена`
                                FROM `product` 
	                            LEFT JOIN `category` ON `product`.`Категория` = `category`.`category_id`
                                WHERE "
                ;
                foreach (string s in words)
                {
                    DataBase.sqlcmd += $"`product`.`Наименование` LIKE '%{s}%' OR `category`.`Категория` LIKE '%{s}%' OR";
                }
                DataBase.sqlcmd = DataBase.sqlcmd.Remove(DataBase.sqlcmd.Length - 3);
                button1.Content = "Сбросить";
                search_prod = true;
            }
            else if (search_prod == true || textBox_prodSearch.Text == "")
            {
                DataBase.sqlcmd = @"SELECT `product`.`product_id`, `category`.`Категория`, `product`.`Наименование`, `product`.`На складе`, `product`.`Цена`
                                FROM `product` 
	                            LEFT JOIN `category` ON `product`.`Категория` = `category`.`category_id`"
                ;
                button1.Content = "Найти";
                textBox_prodSearch.Text = "";
                search_prod = false;
            }

            DataBase.dt_clients = dataBase.Connect(DataBase.sqlcmd);

            dataGrid2.Items.Clear();
            for (int i = 0; i < DataBase.dt_clients.Rows.Count; i++)
            {
                Product product = new Product()
                {
                    Product_id = Convert.ToString(DataBase.dt_clients.Rows[i][0]),
                    Category = Convert.ToString(DataBase.dt_clients.Rows[i][1]),
                    Title = Convert.ToString(DataBase.dt_clients.Rows[i][2]),
                    Count = Convert.ToString(DataBase.dt_clients.Rows[i][3]),
                    Cost = Convert.ToString(DataBase.dt_clients.Rows[i][4]),
                };

                dataGrid2.Items.Add(product);

            }

        }

        // Изменение параметров поиска товаров:
        private void textBox_prodSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            button1.Content = "Найти";
            search_prod = false;
        }

        // Офромление заказа:
        private void button_Click(object sender, RoutedEventArgs e)
        {
            Order order = new Order();
            order.label_Date.Content = DateTime.Now.ToString("dd/MM/yyyy");

            order.label_totalCost.Content = costcount;
            order.add_cart();
            order.ShowDialog();
            Load_Products();
            label1.Content = "";

            order.comboBox1.Visibility = Visibility.Hidden;
            order.button2.Visibility = Visibility.Hidden;
            order.button.Visibility = Visibility.Visible;

            dataGrid_orders.Items.Clear();
        }


        #endregion Заказ

    }

    // Товары:
    class Product
    {
        public string Product_id { get; set; }
        public string Category { get; set; }
        public string Title { get; set; }
        public string Count { get; set; }
        public string Cost { get; set; }
    }

    // Корзина:
    class Cart
    {
        public string Cart_id { get; set; }
        public string Product_id_inCart { get; set; }
        public string Count_inCart { get; set; }
        public string Cost_inCart { get; set; }
        public string maxCount { get; set; }
    }

    // Клиенты:
    class Clients
    {
        public string Client_id { get; set; }
        public string FIO { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string BDay { get; set; }
    }

    // Заказы:
    class Orders
    {
        public string Order_id { get; set; }
        public string Client_id { get; set; }
        public string Order_Date { get; set; }
        public string Total_cost { get; set; }
        public string Order_status { get; set; }
    }
}
