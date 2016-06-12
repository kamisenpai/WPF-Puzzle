using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace WPF_Puzzle
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Piece[,] table = new Piece[7, 7];
        Point pos = new Point(0, 0);
        bool isRunning = false;        


        public MainWindow()
        {
            InitializeComponent();

        }

        //initializeaza jocul
        private void StartGame(int pieces)
        {
            int pieceSizeWidth = (int)myGrid.Width / pieces; int pieceSizeHeight = (int)myGrid.Height / pieces;
            pos.X = pieces - 1;
            pos.Y = pieces - 1;

            LoadImage();
            if (image.Source != null)
            {
                image.Stretch = Stretch.UniformToFill;
                isRunning = true;
                initTable(pieces, pieceSizeWidth, pieceSizeHeight, image);
                Shuffle();
                slValue.IsEnabled = false;
                btn_imgprev.IsEnabled = true;
                btn_reset.IsEnabled = true;
                txt_diffic.IsEnabled = false;
                btn_start.IsEnabled = false;
            }
            else btn_imgprev.IsEnabled = false;
        }
        //incarca imaginea
        private void LoadImage()
        {
            BitmapImage src = new BitmapImage();
            OpenFileDialog OFD = new OpenFileDialog();
            OFD.Title = " Load an Image";
            OFD.Filter = "All JPG Images (*.jpg) | *.JPG|All PNG Images (*.png) | *.PNG|All files (*.*)| *.*";
            if (OFD.ShowDialog() == true)
            {
                try
                {
                    btnEmpty();
                    src.BeginInit();
                    src.UriSource = new Uri(OFD.FileName, UriKind.Relative);
                    src.CacheOption = BitmapCacheOption.OnLoad;
                    src.EndInit();
                    image.Stretch = Stretch.UniformToFill;
                    image.Source = src;
                }
                catch
                {
                    image.Source = null;
                    MessageBox.Show("The image is not valid.", "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                image.Source = null;
                MessageBox.Show("You didn't picked a image.", "Informatie", MessageBoxButton.OK, MessageBoxImage.Information);
            }

        }

        //Taie imaginea pentru a o pune in buton
        private ImageBrush CropImage(Image img, int fromX, int fromY, int WidthSize, int HeightSize)
        {
            ImageBrush ib = new ImageBrush();
            ib.Stretch = Stretch.UniformToFill;
            BitmapSource bmpImage;
            bmpImage = (BitmapImage)img.Source;
            bmpImage = (BitmapImage)ResizeImage(img, (int)myGrid.Width, (int)myGrid.Height).Source;       
            //bmpImage = (BitmapImage)img.Source;
            bmpImage = GenerateCroppedImage(bmpImage, new Point(fromX, fromY), WidthSize, HeightSize);
            ib.ImageSource = bmpImage;
            return ib;

        }

        //Reface imaginea pentru a se potrivi cu dimensiunea spatiului de joc
        private Image ResizeImage(Image img, int Width, int Height)
        {
            Image resizedImage = new Image();
            resizedImage = img;
            resizedImage.Height = Height;
            resizedImage.Width = Width;
            resizedImage.Stretch = Stretch.Fill;
            return resizedImage;
        }

        //Taie imaginea in bucati si insereaza bucatile in butoane
        public BitmapSource GenerateCroppedImage(BitmapSource source, Point centerPoint, double width, double height)
        {
            int pieces = (int)Math.Sqrt(myGrid.Children.Count);
            double sourceDpiX = source.DpiX;
            double sourceDpiY = source.DpiY;

            double wpfUnitsX = 96;
            double wpfUnitsY = 96;

            double centerPointXInScreenUnits = centerPoint.X / sourceDpiX * wpfUnitsX;
            double centerPointYInScreenUnits = centerPoint.Y / sourceDpiY * wpfUnitsY;

            int targetWidth = (int)Math.Round(width, 0);
            int targetHeight = (int)Math.Round(height, 0);

            double targetWidthInScreenUnits = source.PixelWidth / sourceDpiX * wpfUnitsX;
            double targetHeightInScreenUnits = source.PixelHeight / sourceDpiY * wpfUnitsY;

            //double sourceWidthInScreenUnits = source.PixelWidth / sourceDpiX * wpfUnitsX;
            //double sourceHeightInScreenUnits = source.PixelHeight / sourceDpiY * wpfUnitsY;

            // Muta punctele in dreapta sus
            TranslateTransform translateTransform = new TranslateTransform();
            translateTransform.X = -1 * (centerPointXInScreenUnits );
            translateTransform.Y = -1 * (centerPointYInScreenUnits );

            TransformGroup transformGroup = new TransformGroup();

            transformGroup.Children.Add(translateTransform);

            //Intinde imaginea astfel incat sa incapa pe butoane
            Image image = new Image();
            image.Stretch = Stretch.None;
            image.Source = source;
            image = ResizeImage(image, (int)myGrid.Width, (int)myGrid.Height);
            image.RenderTransform = transformGroup;

            // Picteaza imaginile
            Canvas container = new Canvas();
            container.Children.Add(image);
            container.Arrange(new Rect(0, 0, source.PixelWidth, source.PixelHeight));
            // Creeaza sursa Bitmap
            RenderTargetBitmap target = new RenderTargetBitmap(targetWidth, targetHeight, sourceDpiX, sourceDpiY, PixelFormats.Default);
            target.Render(container);
            return target;
        }
      

        //initializeaza matricea de butoane 
        private void initTable(int pieces, int pieceSizeWidth, int pieceSizeHeight, Image img)
        {
            int i, j;
            int k = 1;
            Point piecePos = new Point(0, 0);
            for (i = 0; i < pieces; i++)
            {
                for (j = 0; j < pieces; j++)
                {
                    table[i, j] = new Piece(pieceSizeWidth, pieceSizeHeight);
                    table[i, j].VerticalAlignment = VerticalAlignment.Top;
                    table[i, j].HorizontalAlignment = HorizontalAlignment.Left;
                    table[i, j].Margin = new Thickness(piecePos.X, piecePos.Y, 0, 0);
                    table[i, j].Tag = k;
                    table[i, j].Pos(new Point(i, j));
                    table[i, j].FontWeight = FontWeights.UltraBold;
                    table[i, j].Background = CropImage(image, (int)piecePos.X, (int)piecePos.Y, pieceSizeWidth, pieceSizeHeight);
                    table[i, j].Content = ((int)table[i, j].Tag).ToString();
                    table[i, j].PreviewMouseDown += new MouseButtonEventHandler(table_MouseDown);
                    myGrid.Children.Add(table[i, j]);
                    piecePos.X += pieceSizeWidth;
                    k++;
                }
                piecePos.X = 0;
                piecePos.Y += pieceSizeHeight;
            }
            table[pieces - 1, pieces - 1].Content = null;
            table[pieces - 1, pieces - 1].Background = null;
        }



        private void table_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Piece btnCurrent = sender as Piece;
            Piece btnBlank = table[(int)pos.X,(int)pos.Y];
            bool ok = false;

            //Swap(btnCurrent, btnBlank);
            int currentTileRow = (int)btnCurrent.PositionX;
            int currentTileCol = (int)btnCurrent.PositionY;
            int currentBlankRow = (int)pos.X;
            int currentBlankCol = (int)pos.Y;

            List<PossiblePositions> posibilities = new List<PossiblePositions>();
            posibilities.Add(new PossiblePositions
            { Row = currentBlankRow - 1, Col = currentBlankCol });
            posibilities.Add(new PossiblePositions
            { Row = currentBlankRow + 1, Col = currentBlankCol });
            posibilities.Add(new PossiblePositions
            { Row = currentBlankRow, Col = currentBlankCol - 1 });
            posibilities.Add(new PossiblePositions
            { Row = currentBlankRow, Col = currentBlankCol + 1 });
            btnCurrent.IsDefault = true;

            bool validMove = false;
            foreach (PossiblePositions position in posibilities)
                if (currentTileRow == position.Row && currentTileCol == position.Col)
                    validMove = true;
            if (isRunning)
            {
                if (validMove)
                {
                    //Swap(btnCurrent, btnBlank);
                    if (btnCurrent.PositionX < pos.X)
                    {
                        Swap((int)pos.X, (int)pos.Y, (int)btnCurrent.PositionX, (int)btnCurrent.PositionY);
                        pos.X--;
                        ok = true;
                    }
                    else if (btnCurrent.PositionY < pos.Y)
                    {
                        Swap((int)pos.X, (int)pos.Y, (int)btnCurrent.PositionX, (int)btnCurrent.PositionY);
                        pos.Y--;
                        ok = true;
                    }
                    else if (btnCurrent.PositionX > pos.X)
                    {
                        Swap((int)pos.X, (int)pos.Y, (int)btnCurrent.PositionX, (int)btnCurrent.PositionY);
                        pos.X++;
                        ok = true;
                    }
                    else if (btnCurrent.PositionY > pos.Y)
                    {
                        Swap((int)pos.X, (int)pos.Y, (int)btnCurrent.PositionX, (int)btnCurrent.PositionY);
                        pos.Y++;
                        ok = true;

                    }
                }
            }

            if (ok == true)
            {
                table[(int)pos.X, (int)pos.Y].IsDefault = true;
                CheckTable();
            }

        }

        //Muta butoanele, unul in locul celuilalt
        private void Swap(int x1, int y1, int x2, int y2)
        {
            Piece aux = new Piece(table[x1, y1].Width, table[x1, y1].Height);
            aux.Background = table[x1, y1].Background;
            aux.Content = table[x1, y1].Content;
            aux.Tag = table[x1, y1].Tag;

            table[x1, y1].Tag = table[x2, y2].Tag;
            table[x1, y1].Content = table[x2, y2].Content;
            table[x1, y1].Background = table[x2, y2].Background;

            table[x2, y2].Tag = aux.Tag;
            table[x2, y2].Content = aux.Content;
            table[x2, y2].Background = aux.Background;
            aux = null;
        }

        //Goleste matricea de butoane
        private void btnEmpty()
        {
            int k;
            int pieces = (int)Math.Sqrt(myGrid.Children.Count);
            for (k = myGrid.Children.Count - 1; k >= 0; k--)
            {
                myGrid.Children.RemoveAt(k);
            }
        }
        private void btn_start_Click(object sender, RoutedEventArgs e)
        {
            StartGame((int)slValue.Value);
        }

        struct PossiblePositions
        {
            public int Row { get; set; }
            public int Col { get; set; }
        }

        //Verifica daca Conditiile de joc au fost indeplinite
        private void CheckTable()
        {
            int i, j;
            int pieces = (int)Math.Sqrt(myGrid.Children.Count);
            bool ok = true;
            int k = pieces * pieces;
            if (isRunning)
            {
                for (i = pieces - 1; i >= 0; i--)
                {
                    for (j = pieces - 1; j >= 0; j--)
                    {
                        if ((int)table[i, j].Tag != k)
                        {
                            ok = false;
                            break;
                        }
                        k--;
                    }
                    if (ok == false)
                        break;
                }
                if (ok == true)
                {
                    var newGame = MessageBox.Show("Congratulations, you won!", "Congratulations", MessageBoxButton.OK, MessageBoxImage.Information);
                    isRunning = false;
                    slValue.IsEnabled = true;
                    txt_diffic.IsEnabled = true;
                }
            }

        }

        //Amesteca toate piesele folosing functia
        private void Shuffle()
        {
            int i;
            int pieces = (int)Math.Sqrt(myGrid.Children.Count);
            Random rnd = new Random();
            for (i = 0; i < pieces * pieces; i++)
            {
                int x1 = rnd.Next(0, pieces);
                int x2 = rnd.Next(0, pieces);
                int y1 = rnd.Next(0, pieces);
                int y2 = rnd.Next(0, pieces);
                if ((x1 != pos.X & y1 != pos.Y) || (x2 != pos.X & y2 != pos.Y))
                    Swap(x1, x2, y1, y2);
            }
        }

        // De fiecare data cand se muta o piesa, se verifica daca conditiile de castig sunt indeplinite
        private void Game_Window_KeyDown(object sender, KeyEventArgs e)
        {
            int pieces = (int)Math.Sqrt(myGrid.Children.Count);

            bool ok = false;

            if (isRunning)
            {
                switch (e.Key)
                {
                    case Key.S:
                        if (pos.X > 0)
                        {
                            Swap((int)pos.X, (int)pos.Y, (int)pos.X - 1, (int)pos.Y);
                            pos.X -= 1;
                            ok = true;
                        }
                        break;
                    case Key.D:
                        if (pos.Y > 0)
                        {
                            Swap((int)pos.X, (int)pos.Y, (int)pos.X, (int)pos.Y - 1);
                            pos.Y -= 1;
                            ok = true;
                        }
                        break;
                    case Key.W:
                        if (pos.X < pieces - 1)
                        {
                            Swap((int)pos.X, (int)pos.Y, (int)pos.X + 1, (int)pos.Y);
                            pos.X += 1;
                            ok = true;
                        }
                        break;
                    case Key.A:
                        if (pos.Y < pieces - 1)
                        {
                            Swap((int)pos.X, (int)pos.Y, (int)pos.X, (int)pos.Y + 1);
                            pos.Y += 1;
                            ok = true;
                        }
                        break;
                }
            }
            if (ok == true)
            {
                table[(int)pos.X, (int)pos.Y].IsDefault = true;
                CheckTable();
            }
        }

        //Verifica daca in textoboxul de la dificultate s-au introdus alte caractere inafara de cfre
        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            foreach (char c in e.Text)
            {
                if (!char.IsDigit(c))
                {
                    e.Handled = true;
                    break;
                }
            }
        }

        //Reseteaza totul
        private void btn_reset_Click(object sender, RoutedEventArgs e)
        {
            btnEmpty();
            image.Source = null;
            if (previewWindow != null)
                previewWindow.Close();       
            slValue.IsEnabled = true;
            txt_diffic.IsEnabled = true;
            btn_imgprev.IsEnabled = false;
            btn_start.IsEnabled = true;
            btn_reset.IsEnabled = false;
            isRunning = false;
        }

        //Arata imaginea sursa intr-o fereastra, nemodificata
        private Preview_Window previewWindow = null;
        private void btn_imgprev_Click(object sender, RoutedEventArgs e)
        {
            var desktopWorkingArea = System.Windows.SystemParameters.WorkArea;
          
            img_piece.Source = image.Source;
            if (img_piece.Source != null && previewWindow == null)
            {
                previewWindow = new Preview_Window(img_piece);
                btn_imgprev.IsEnabled = false;
                previewWindow.Closed += Preview_WindowClosed;
                previewWindow.Left = desktopWorkingArea.Right - Width;
                previewWindow.Top = desktopWorkingArea.Top+100; 
                previewWindow.Show();
                Game_Window.Focus();
            }
            else
            {
                MessageBox.Show("Load an image first!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        //Inchide toata aplicatia chiar daca mai sunt alte window-uri deschise
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            Application.Current.Shutdown();
        }

        //Cand se inchide fereastra cu priview face ceva
        public void Preview_WindowClosed(object sender, System.EventArgs e)
        {
            btn_imgprev.IsEnabled = true;
            previewWindow = null;

        }

        private void btn_help_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("This is a slider puzzle. You can select the difficulty at the beginning with the slider on the right or you can write the difficulty in the textbox, it goes from 3 to 7 (a.k.a. 3x3 to 7x7) \n \n Once you have set the difficulty you press the 'start game' button and you need to choose a jpg photo from your PC.After that the game will start and you can move the empty piece with the keyboard buttons 'WASD' or by clicking the piece you want to move.When you put all the pieces together, you will win the game. \n \n Also, you can preview your photo if you want and if you want to reset the game, just press the 'reset' button and you can choose another photo or another difficulty.", "MVP exam project in WPF C#", MessageBoxButton.OK,MessageBoxImage.Information);
        }
        // previewWindow.Show();         
    }
}

