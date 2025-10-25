namespace COM3D2.DressCode;

internal static class Mpn {
	public static readonly MPN null_mpn = Parse("null_mpn");
	public static readonly MPN body = Parse("body");
	public static readonly MPN hairf = Parse("hairf");
	public static readonly MPN hairr = Parse("hairr");
	public static readonly MPN hairt = Parse("hairt");
	public static readonly MPN hairs = Parse("hairs");
	public static readonly MPN hairaho = Parse("hairaho");
	public static readonly MPN wear = Parse("wear");
	public static readonly MPN skirt = Parse("skirt");
	public static readonly MPN mizugi = Parse("mizugi");
	public static readonly MPN bra = Parse("bra");
	public static readonly MPN panz = Parse("panz");
	public static readonly MPN stkg = Parse("stkg");
	public static readonly MPN shoes = Parse("shoes");
	public static readonly MPN headset = Parse("headset");
	public static readonly MPN glove = Parse("glove");
	public static readonly MPN acchead = Parse("acchead");
	public static readonly MPN accha = Parse("accha");
	public static readonly MPN acchana = Parse("acchana");
	public static readonly MPN acckamisub = Parse("acckamisub");
	public static readonly MPN acckami = Parse("acckami");
	public static readonly MPN accmimi = Parse("accmimi");
	public static readonly MPN accnip = Parse("accnip");
	public static readonly MPN acckubi = Parse("acckubi");
	public static readonly MPN acckubiwa = Parse("acckubiwa");
	public static readonly MPN accheso = Parse("accheso");
	public static readonly MPN accude = Parse("accude");
	public static readonly MPN accashi = Parse("accashi");
	public static readonly MPN accsenaka = Parse("accsenaka");
	public static readonly MPN accshippo = Parse("accshippo");
	public static readonly MPN accanl = Parse("accanl");
	public static readonly MPN accvag = Parse("accvag");
	public static readonly MPN megane = Parse("megane");
	public static readonly MPN accxxx = Parse("accxxx");
	public static readonly MPN handitem = Parse("handitem");
	public static readonly MPN acchat = Parse("acchat");
	public static readonly MPN onepiece = Parse("onepiece");
	public static readonly MPN set_maidwear = Parse("set_maidwear");
	public static readonly MPN set_mywear = Parse("set_mywear");
	public static readonly MPN set_underwear = Parse("set_underwear");

	public static MPN Parse(string name) => (MPN)Enum.Parse(typeof(MPN), name);
}
