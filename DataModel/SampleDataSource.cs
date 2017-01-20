using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.ApplicationModel.Resources.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using System.Collections.Specialized;

// El modelo de datos definido por este archivo sirve como ejemplo representativo de un modelo
// fuertemente tipado que admite notificación cuando se agregan, quitan o modifican miembros. Los nombres
// de propiedad elegidos coinciden con enlaces de datos en las plantillas de elemento estándar.
//
// Las aplicaciones pueden usar este modelo como punto de inicio y ampliarlo o descartarlo completamente
// y reemplazarlo por algo adecuado a sus necesidades.

namespace Trabajo_Catalogo3.Data
{
    /// <summary>
    /// Clase base para <see cref="SampleDataItem"/> y <see cref="SampleDataGroup"/> que
    /// define propiedades comunes a ambos.
    /// </summary>
    [Windows.Foundation.Metadata.WebHostHidden]
    public abstract class SampleDataCommon : Trabajo_Catalogo3.Common.BindableBase
    {
        private static Uri _baseUri = new Uri("ms-appx:///");

        public SampleDataCommon(String uniqueId, String title, String subtitle, String imagePath, String description)
        {
            this._uniqueId = uniqueId;
            this._title = title;
            this._subtitle = subtitle;
            this._description = description;
            this._imagePath = imagePath;
        }

        private string _uniqueId = string.Empty;
        public string UniqueId
        {
            get { return this._uniqueId; }
            set { this.SetProperty(ref this._uniqueId, value); }
        }

        private string _title = string.Empty;
        public string Title
        {
            get { return this._title; }
            set { this.SetProperty(ref this._title, value); }
        }

        private string _subtitle = string.Empty;
        public string Subtitle
        {
            get { return this._subtitle; }
            set { this.SetProperty(ref this._subtitle, value); }
        }

        private string _description = string.Empty;
        public string Description
        {
            get { return this._description; }
            set { this.SetProperty(ref this._description, value); }
        }

        private ImageSource _image = null;
        private String _imagePath = null;
        public ImageSource Image
        {
            get
            {
                if (this._image == null && this._imagePath != null)
                {
                    this._image = new BitmapImage(new Uri(SampleDataCommon._baseUri, this._imagePath));
                }
                return this._image;
            }

            set
            {
                this._imagePath = null;
                this.SetProperty(ref this._image, value);
            }
        }

        public void SetImage(String path)
        {
            this._image = null;
            this._imagePath = path;
            this.OnPropertyChanged("Image");
        }

        public override string ToString()
        {
            return this.Title;
        }
    }

    /// <summary>
    /// Modelo de datos de elemento genérico.
    /// </summary>
    public class SampleDataItem : SampleDataCommon
    {
        public SampleDataItem(String uniqueId, String title, String subtitle, String imagePath, String description, String content, SampleDataGroup group)
            : base(uniqueId, title, subtitle, imagePath, description)
        {
            this._content = content;
            this._group = group;
        }

        private string _content = string.Empty;
        public string Content
        {
            get { return this._content; }
            set { this.SetProperty(ref this._content, value); }
        }

        private SampleDataGroup _group;
        public SampleDataGroup Group
        {
            get { return this._group; }
            set { this.SetProperty(ref this._group, value); }
        }
    }

    /// <summary>
    /// Modelo de datos de grupo genérico.
    /// </summary>
    public class SampleDataGroup : SampleDataCommon
    {
        public SampleDataGroup(String uniqueId, String title, String subtitle, String imagePath, String description)
            : base(uniqueId, title, subtitle, imagePath, description)
        {
            Items.CollectionChanged += ItemsCollectionChanged;
        }

        private void ItemsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // Proporciona un subconjunto de la colección completa de elementos al que enlazar desde un elemento GroupedItemsPage
            // por dos motivos: GridView no virtualiza colecciones de elementos grandes y
            // mejora la experiencia del usuario cuando examina grupos con números grandes de
            // elementos.
            //
            // Se muestra un máximo de 12 elementos porque da lugar a columnas de cuadrícula rellenas
            // ya se muestren 1, 2, 3, 4 o 6 filas

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewStartingIndex < 12)
                    {
                        TopItems.Insert(e.NewStartingIndex,Items[e.NewStartingIndex]);
                        if (TopItems.Count > 12)
                        {
                            TopItems.RemoveAt(12);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    if (e.OldStartingIndex < 12 && e.NewStartingIndex < 12)
                    {
                        TopItems.Move(e.OldStartingIndex, e.NewStartingIndex);
                    }
                    else if (e.OldStartingIndex < 12)
                    {
                        TopItems.RemoveAt(e.OldStartingIndex);
                        TopItems.Add(Items[11]);
                    }
                    else if (e.NewStartingIndex < 12)
                    {
                        TopItems.Insert(e.NewStartingIndex, Items[e.NewStartingIndex]);
                        TopItems.RemoveAt(12);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (e.OldStartingIndex < 12)
                    {
                        TopItems.RemoveAt(e.OldStartingIndex);
                        if (Items.Count >= 12)
                        {
                            TopItems.Add(Items[11]);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    if (e.OldStartingIndex < 12)
                    {
                        TopItems[e.OldStartingIndex] = Items[e.OldStartingIndex];
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    TopItems.Clear();
                    while (TopItems.Count < Items.Count && TopItems.Count < 12)
                    {
                        TopItems.Add(Items[TopItems.Count]);
                    }
                    break;
            }
        }

        private ObservableCollection<SampleDataItem> _items = new ObservableCollection<SampleDataItem>();
        public ObservableCollection<SampleDataItem> Items
        {
            get { return this._items; }
        }

        private ObservableCollection<SampleDataItem> _topItem = new ObservableCollection<SampleDataItem>();
        public ObservableCollection<SampleDataItem> TopItems
        {
            get {return this._topItem; }
        }
    }

    /// <summary>
    /// Crea una colección de grupos y elementos con contenido codificado de forma rígida.
    /// 
    /// SampleDataSource se inicializa con datos de marcadores de posición en lugar de con datos de producción
    /// activos, por lo que se proporcionan datos de muestra tanto en el momento del diseño como en el de la ejecución.
    /// </summary>
    public sealed class SampleDataSource
    {
        private static SampleDataSource _sampleDataSource = new SampleDataSource();

        private ObservableCollection<SampleDataGroup> _allGroups = new ObservableCollection<SampleDataGroup>();
        public ObservableCollection<SampleDataGroup> AllGroups
        {
            get { return this._allGroups; }
        }

        public static IEnumerable<SampleDataGroup> GetGroups(string uniqueId)
        {
            if (!uniqueId.Equals("AllGroups")) throw new ArgumentException("Only 'AllGroups' is supported as a collection of groups");
            
            return _sampleDataSource.AllGroups;
        }

        public static SampleDataGroup GetGroup(string uniqueId)
        {
            // La búsqueda lineal sencilla es aceptable para conjuntos de datos reducidos
            var matches = _sampleDataSource.AllGroups.Where((group) => group.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        public static SampleDataItem GetItem(string uniqueId)
        {
            // La búsqueda lineal sencilla es aceptable para conjuntos de datos reducidos
            var matches = _sampleDataSource.AllGroups.SelectMany(group => group.Items).Where((item) => item.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        public SampleDataSource()
        {
         
        //Creacion de los grupos

            var group1 = new SampleDataGroup("Group-1",
                    "Plantas Medicinales",
                    "Grupo 1",
                    "Assets/plantas.jpg",
                    "Una planta medicinal es un recurso, cuya parte o extractos se emplean como drogas en el tratamiento de alguna afección. La parte de la planta empleada medicinalmente se conoce con el nombre de droga vegetal, y puede suministrarse bajo diferentes formas galénicas: cápsulas, comprimidos, crema, elixir, infusión, jarabe, tintura, ungüento, etc.");

            var group2 = new SampleDataGroup("Group-2",
                    "Plantas Medicinales",
                    "Grupo 2",
                    "Assets/plantas.jpg",
                    "Una planta medicinal es un recurso, cuya parte o extractos se emplean como drogas en el tratamiento de alguna afección. La parte de la planta empleada medicinalmente se conoce con el nombre de droga vegetal, y puede suministrarse bajo diferentes formas galénicas: cápsulas, comprimidos, crema, elixir, infusión, jarabe, tintura, ungüento, etc.");

            var group3 = new SampleDataGroup("Group-3",
                    "Plantas Medicinales",
                    "Grupo 3",
                    "Assets/plantas.jpg",
                    "Una planta medicinal es un recurso, cuya parte o extractos se emplean como drogas en el tratamiento de alguna afección. La parte de la planta empleada medicinalmente se conoce con el nombre de droga vegetal, y puede suministrarse bajo diferentes formas galénicas: cápsulas, comprimidos, crema, elixir, infusión, jarabe, tintura, ungüento, etc.");
         
  

       //Comenzar a agregar los productos como items en el grupo     
       
//Agregar al Grupo 1     

            group1.Items.Add(new SampleDataItem("Group-1-Item-1",
                    "Albahaca",
                    "Ocimum basilicum",
                    "Assets/imagenes grandes/1) Albahaca.jpg",
                    "",
                    "Es una planta herbácea, aromática anual o bianual, según las condiciones del ambiente donde se encuentre. Las hojas son jugosas, aromáticas, pccioladas opuestas, finamente dentadas o aserradas y ovaladas. Tallos erguidos, ramillados, de hasta 50 cm. de alto, cuadrangulares. \n\n\tLas flores dispuestas en la parte superior del tallo o en el extremo de las ramas, son de color blanco aunque existen púrpuras pálido, se hallan dispuestas en espigas axilares.",
                    group1));
          
            group1.Items.Add(new SampleDataItem("Group-1-Item-2",
                    "Aloe Vera",
                    "Aloe barbadensis",
                    "Assets/imagenes grandes/2) Aloe Vera.jpg",
                    "", 
                    "El Aloe Vera proviene principalmente de África y Asia. El Aloe Vera, tiene dos enemigos naturales: el exceso de agua y el frío por debajo de lo 10ºC. Por otro lado, es muy resistente a la plagas y a la falta de agua. \n\n\tLas propiedades curativas del aloe se manifiestan cuando la planta llega a la edad adulta, hecho que sucede a los tres años, aproximadamente desde que han hecho la flor.",
                    group1));
          
            
            group1.Items.Add(new SampleDataItem("Group-1-Item-3",
                    "Boldo",
                    "Peumus boldus",
                    "Assets/imagenes grandes/3) Boldo.jpg",
                    "", 
                    "El boldo (Peumus boldus) es originario de Chile y se ha adaptado muy bien a los climas mediterráneos y se ha convertido con el transcurrir del tiempo en una de las más famosas plantas con propiedades fitoterapéuticas. \n\n\tEn algunos lugares es utilizado como tónico digestivo a manera de té. La infusión de sus hojas estimula la secreción biliar y el proceso digestivo. Posee cualidades como diurético.",
                    group1));
           
            
            group1.Items.Add(new SampleDataItem("Group-1-Item-4",
                    "Caña Agria",
                    "Costus spicatus",
                    "Assets/imagenes grandes/4) Caña Agria.jpg",
                    "",
                    "Es originaria de México, se da en climas cálidos, semicálidos, y templados, se desarrolla y crece con una mejor apariencia estando en la sombra y que solo le dé un poco de sol.\n\n\tSe conoce muy poco acerca de la fitoquímica de esta planta, los estudios que se han realizado sobre la química de C. spicatus han reportado presencia de cianidin, camferol, delfinidin, flavonoides y quercetina en las hojas.",
                    group1));
            group1.Items.Add(new SampleDataItem("Group-1-Item-5",
                    "Culen",
                    "Psoralea glandulosa",
                    "Assets/imagenes grandes/5) Culen.jpg",
                    "",
                    "Es nativo de Chile y crece en los lugares húmedos de los valles y las quebradas de las cordilleras. Es un arbusto siempre verde de ramas delgadas y corteza verdosa. Las propiedades que posee se deben al tanino que tiene propidades astringentes.\n\n\tSe usa para aliviar las diarreas, asi como tambíen para lavar heridas y como antihelmíntico. La raíz tiene propiedades eméticas. Además es utilizada como antidiabético, pero esto no ha sido probado todavía.",
                    group1));

            group1.Items.Add(new SampleDataItem("Group-1-Item-6",
                   "Flor de Azahar",
                   "Citrus aurantiifolia",
                   "Assets/imagenes grandes/6) Flor de azahar.jpg",
                   "",
                   "La flor de azahar es la flor del naranjo, generalmente del naranjo amargo y posee milenarias propiedades medicinales para tratar problemas de insomnio y nerviosismo.\n\n\tEn general, sus efectos calmantes ayudan a eliminar molestias causadas por nervios. El agua de azahar (una infusión de pétalos secos de azahar), es empleada como remedio tradicional contra desmayos. También ha sido empleada durante siglos como remedio casero para las molestias menstruales.",
                   group1));


            group1.Items.Add(new SampleDataItem("Group-1-Item-7",
                   "Helecho Macho",
                   "Dryopteris Filix-Mas",
                   "Assets/imagenes grandes/7) Helecho macho.jpg",
                   "",
                   "El helecho macho es una planta originaria de las zonas templadas del hemisferio norte. Se encuentra en bosques sombríos y húmedos y a las orillas de los arroyos. Es mucho menos abundante en Norteamérica que en Europa. En España se encuentra desde Sierra Nevada hasta Pirineos, pasando por Cataluña, Galicia y Portugal, aunque no abunda tanto como otros helechos.",
                   group1));

            group1.Items.Add(new SampleDataItem("Group-1-Item-8",
                   "Hinojo",
                   "Foeniculum vulgare",
                   "Assets/imagenes grandes/8) Hinojo.jpg",
                   "",
                   "El hinojo es una planta muy similar al apio pero con un sabor parecido al del anís. Uno de los usos mas conocidos del hinojo es su propiedad para reducir el apetito. Este no solo era utilizado en la batalla, sino también por los monjes que la utilizaban en épocas de ayuno. El hinojo tiene también propiedades expectorantes, mejora la visión y los malestares estomacales.",
                   group1));

            group1.Items.Add(new SampleDataItem("Group-1-Item-9",
                   "Guaco",
                   "Mikania guaco",
                   "Assets/imagenes grandes/9) Guaco.jpg",
                   "",
                   "El guaco es una planta tipo enredadera que es originaria de América, posee flores blancas, y tallo leñoso;  sus usos medicinales y terapéuticos son enfocados especialmente en las propiedades de las hojas.\n\n\tSus acciones terapéuticas se ejercen directamente para ayudar en problemas de la piel (ayuda en la curación de heridas, lesiones, picaduras y otras), desordenes diuréticos, dolores en el cuerpo, enfermedades en el sistema respiratorio (tos, gripa, bronquitis, esofagitis, asma, etc).",
                   group1));


            group1.Items.Add(new SampleDataItem("Group-1-Item-10",
                   "Flor de Jamaica",
                   "Hibiscus sabdariffa",
                   "Assets/imagenes grandes/10) Jamaica.jpg",
                   "",
                   "La flor de jamaica o rosa de jamaica es un tipo de planta llamado hibisco. La flor de jamaica se utiliza para calmar los síntomas de la gripa, curar enfermedades estomacales y en el tratamiento de enfermedades del corazón; puede ser muy útil para mejorar problemas de la piel como úlceras, lesiones y abscesos. Sirve para tratar problemas menstruales tal como regular los ciclos de la menstruación.",
                   group1));

//Agregar al Grupo 2


            group2.Items.Add(new SampleDataItem("Group-2-Item-11",
                   "Llanten",
                   "Plantago major",
                   "Assets/imagenes grandes/11) Llanten.jpg",
                   "",
                   "El llantén es una planta originaria de Europa, aunque es posible encontrarla creciendo silvestre en las zonas templadas y frías de todo el planeta. Es Antitusígeno y expectorante, por lo que disminuye síntomas del resfrío, asma, neumonías, faringitis, laringitis, bronquitis, tuberculosis, etc. Sus propiedades descongestionantes y expectorantes suaves son muy útiles para desinflamar las vías respiratorias y ayudar a expulsar las mucosidades que allí se desarrollan.",
                   group2));



            group2.Items.Add(new SampleDataItem("Group-2-Item-12",
                   "Manzanilla",
                   "Matricaria chamomilla L.",
                   "Assets/imagenes grandes/12) Manzanilla.jpg",
                   "",
                   "La manzanilla es una hierba aromática que a sido utilizada desde hace siglos con fines medicinales. Es originaria de Europa y de allí introducida a América, donde es muy comercializada y cultivada.\n\n\tEs un antinflamatorio. Ayuda a aliviar los procesos de inflamación tanto tomándola o usándola externamente, se le utiliza para hacer gárgaras cuando hay problemas de garganta y de encías y ayuda con la cicatrización. Es calmante y tranquilizante, actúa como un sedante suave. Se le usa en caso de irritabilidad, tristeza y ansiedad. ",
                   group2));




            group2.Items.Add(new SampleDataItem("Group-2-Item-13",
                   "Matico",
                   "Piper aduncum",
                   "Assets/imagenes grandes/13) Matico.jpg",
                   "",
                   "Matico se encuentra desde Santiago a Chiloé (RM a X región), también en Argentina. Frecuente en matorrales y a las orillas de los caminos, prefiere suelos profundos no anegados.\n\n\tLa principal propiedad medicinal de esta planta es la de ayudar en la cicatrización de todo tipo de heridas, ya sea externas o internas. De aquí deriva su utilidad en el tratamiento de la úlcera digestiva. Externamente, su efecto benéfico sobre heridas de lenta cicatrización es muy sorprendente.",
                   group2));


            group2.Items.Add(new SampleDataItem("Group-2-Item-14",
                   "Menta",
                   "Mentha piperita",
                   "Assets/imagenes grandes/14) Menta.jpg",
                   "",
                   "La menta es una planta medicinal muy fácil de conseguir que funciona de forma muy efectiva para aliviar problemas estomacales de manera natural. Ayuda a mejorar la digestión y prevenir las flatulencias. Una infusión de menta después de una comida pesada le sentará muy bien a tu estómago. Esta recomendación está contraindicada si se sufre de gastritis.\n\n\tDebido a sus propiedades antisépticas y expectorantes funciona de forma efectiva aliviando la tos con flema y mejorando mucho la condición de quien la padece.",
                   group2));


            group2.Items.Add(new SampleDataItem("Group-2-Item-15",
                   "Nanche",
                   "Byrsonima crassifolia",
                   "Assets/imagenes grandes/15) Nanche.jpg",
                   "",
                   "Uno de los árboles más comunes de México. Combate desordenes digestivos, cura afecciones de la piel, combate las infecciones en la matriz e inflamación en los ovarios, alivia resfriado y además es antidiarreico",
                   group2));


            group2.Items.Add(new SampleDataItem("Group-2-Item-16",
                   "Noni",
                   "Morinda citrifolia",
                   "Assets/imagenes grandes/16) Noni.jpg",
                   "",
                   "Es un pequeño árbol de hoja perenne de las Islas del Pacífico, del Sudeste de Asia, de India y de Australia y que a menudo crece en terrenos volcánicos. Sus propiedades se aplican tanto para combatir la inflamación y aumentar la resistencia de los gérmenes.\n\n\tSe utiliza en los tratamientos para las personas con diabetes ya sea en adultos y niños. También se utiliza para regular el ciclo menstrual, limpiar el tracto urinario y ayuda con el dolor de la artritis.",
                   group2));



            group2.Items.Add(new SampleDataItem("Group-2-Item-17",
                   "Nopal",
                   "Opuntia ficus-indica",
                   "Assets/imagenes grandes/17) Nopal.jpg",
                   "",
                   "El nopal es una planta que en Chile conocemos como tuna. Pertenece al genero Opuntia y en Chile sólo se acostumbra a consumir los frutos, cuya cosecha es principalmente de febrero en adelante. El principal uso medicinal del nopal hoy en día es contra la diabetes. Las pencas se preparan licuadas con agua para su ingestión o bien se comen crudas o en ensaladas.\n\n\tSe dice que el nopal también es un buen remedio contra la gastritis y los cólicos intestinales, aunque la parte más recomendada para estos males, es la raíz cocida y mezclada con guayaba. Otras aplicaciones, pero menos frecuentes, son para las afecciones de los pulmones y como auxiliar en el parto.",
                   group2));



            group2.Items.Add(new SampleDataItem("Group-2-Item-18",
                   "Paico",
                   "Chenopedicum ambrosioides",
                   "Assets/imagenes grandes/18) Paico.jpg",
                   "",
                   "Es una planta medicinal y aromática usada desde tiempos prehispánicos por los indígenas americanos. Las hojas del paico alivian los cólicos estomacales, resfríos, espasmos, hemorroides, pulmonías, gastritis, dismenorrea, inflamaciones de las vías urinarias, y sirve como antitusígeno, antihelmíntico, purgante, diurético, hepatoprotector, antinflamatorio, antiemético, antiséptico, digestivo y antirreumático.",
                   group2));



            group2.Items.Add(new SampleDataItem("Group-1-Item-19",
                   "Poleo",
                   "Mentha pulegium",
                   "Assets/imagenes grandes/19) Poleo.jpg",
                   "",
                   "El poleo es una planta. El aceite y las hojas se usan con fines medicinales. A lo largo de la historia, tanto el poleo Americano como el poleo Europeo se han utilizado indistintamente como una fuente de aceite de poleo. A pesar de serias preocupaciones sobre su seguridad, el poleo se utiliza para los resfríos, la neumonía y otros problemas respiratorios. También se utiliza para los dolores de estómago, los gases, los trastornos intestinales y los problemas del hígado y de la vesícula biliar.",
                   group2));



            group2.Items.Add(new SampleDataItem("Group-1-Item-20",
                   "Ruda",
                   "Ruta graveolens",
                   "Assets/imagenes grandes/20) Ruda.JPG",
                   "",
                   "La ruda es una planta utilizada desde tiempos inmemoriales en la medicina y se la considera una hierba protectora. Entre las propiedades principales, el té de ruda es un sedante suave que relaja y es bueno contra la ansiedad, el estrés o incluso para dormir mejor. Además se usa como diurético o como espamolítico. Es ideal no sólo para relajarse sino también para personas que no tienen hambre. Es buena la infusión de ruda en caso de trastornos digestivos, cólicos y espasmos estomacales.",
                   group2));


//Agregar al Grupo 3

            group3.Items.Add(new SampleDataItem("Group-3-Item-21",
                   "Romero",
                   "Rosmarinus officinalis",
                   "Assets/imagenes grandes/21) Romero.jpg",
                   "",
                   "El Romero es una planta cuyo consumo se utiliza para tratar diferentes problemas de salud como la hipertensión arterial, sobrepeso o la caída del pelo. Se recomienda para los espasmos vasculares, la insuficiencia circulatoria periférica gracias al alcanfor y sus cualidades vigorizantes a nivel cardiaco y nervioso. Es recomendada para trastornos del apetito, como lo son la anorexia y la inapetencia en general. Es ligeramente diurética.",
                   group3));



            group3.Items.Add(new SampleDataItem("Group-3-Item-22",
                   "Sauco",
                   "Sambucus nigra",
                   "Assets/imagenes grandes/22) Sauco.jpg",
                   "",
                   "El Saúco es una especie abundante en Europa, América, Asia y Norte de África. Siendo los españoles, los que lo llevaron y propagaron por América, donde se halla bien establecido en muchos países. Las bayas del saúco, al igual que diversas bayas de otras plantas, son ricas en antioxidantes. Uno de los beneficios de estos compuestos es mejorar el sistema inmunológico. Para el aparato respiratorio esto proporciona muchos beneficios al ayudar a combatir ciertas enfermedades como la bronquitis, el asma, las reacciones alérgicas y los síntomas de la gripe.",
                   group3));



            group3.Items.Add(new SampleDataItem("Group-3-Item-23",
                   "Tomillo",
                   "Thymus vulgaris",
                   "Assets/imagenes grandes/23) Tomillo.jpg",
                   "",
                   "El tomillo es originario de Asia occidental, Europa Central y el norte de África, pero es posible encontrarla a manera de cultivo en diferentes lugares del planeta. El tomillo posee grandes propiedades como antiséptico, antiespasmódico y antibiótico, por lo cual es frecuentemente utilizado en tratamientos contra procesos digestivos lentos, dolores estomacales, cólicos abdominales, flatulencias y otros trastornos digestivos. También tiene una discreta acción paratisida, y es aconsejada en casos de tenia, oxiuros o áscaris.",
                   group3));



            group3.Items.Add(new SampleDataItem("Group-3-Item-24",
                   "Toronjil",
                   "Melissa officinalis",
                   "Assets/imagenes grandes/24) Toronjil.jpg",
                   "",
                   "El toronjil crece en las zonas tropicales de América del sur. El nombre científico de esta planta 'Melissa' proviene de un vocablo griego antiguo que significa abeja, posiblemente porque el toronjil llamado 'de limón' es una delicia para dichos insectos. Por sus cualidades aromáticas es recomendada para problemas de carácter respiratorio, como es el caso de la rinitis, la gripe y la bronquitis, también es recomendable para los tratamientos contra la hipertensión. Aporta grandes beneficios a nivel respiratorio, digestivo, cardiovascular, ginecológico y dermatológico.",
                   group3));




            group3.Items.Add(new SampleDataItem("Group-3-Item-25",
                   "Violeta",
                   "Viola odorata",
                   "Assets/imagenes grandes/25) Violeta.jpg",
                   "",
                   "Nativa de Europa y Asia, se encuentra en bordes de bosques y clareos, campos sombreados y jardines. Considerada principalmente una planta antitusígena y pectoral. Es empleada especialmente para tratar la tos, el catarro y expulsar las mucosidades; descongestionando con ello las vías respiratorias y contribuyendo a mitigar todos los efectos colaterales que acarrean los resfríos, bronquitis, laringitis, etc. Para ello se puede hacer un cocimiento con sus raíces, endulzándolo con miel o azúcar, o un jarabe con sus flores.",
                   group3)); 
            
            
            //Termina de Agregar los Items
            
            
            
            
            //Agrega el el Grupo creado con los items/productos
            
            this.AllGroups.Add(group1);
            this.AllGroups.Add(group2);
            this.AllGroups.Add(group3);
        }
    }
}
