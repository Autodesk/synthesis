public class Main{
	static {
		ControllerSys.Load();
	}
	public static void main(String[] args){
		System.out.println("No errors:\n");
		var error = new ControllerError();
		var result = ControllerSys.Test(1426, error);
		System.out.println("error_code: " + error.error_code);
		System.out.println("error_message: " + error.error_message);
		System.out.println("error_data: " + error.error_data);
		System.out.println("returned:" + result);

		System.out.println("\nErrors:\n");
		result = ControllerSys.Test(25, error);
		System.out.println("error_code: " + error.error_code);
		System.out.println("error_message: " + error.error_message);
		System.out.println("error_data: " + error.error_data);
		System.out.println("returned:" + result);
	}
}
