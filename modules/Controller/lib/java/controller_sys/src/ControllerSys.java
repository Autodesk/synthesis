import java.io.File;

public class ControllerSys{
	private static boolean Initialized = false;

	public static void Load(){
		if(!Initialized){
			try{
				File lib1 = new File("../../native/controller_sys/target/debug/" + System.mapLibraryName("controller_sys"));
				System.out.println("Loading " + lib1.getAbsolutePath());
				System.load(lib1.getAbsolutePath());
				File lib2 = new File("../../cpp/controller_sys/" + System.mapLibraryName("controller_sys_jni"));
				System.out.println("Loading " + lib2.getAbsolutePath());
				System.load(lib2.getAbsolutePath());
				Initialized = true;
			} catch(Exception ex){
				System.out.println("Error");
				System.out.println(ex.toString());
			}
		}
		System.out.println("Loaded");
	}

	public static native int Test(int value, ControllerError error);
}
