package ad;

import com.ni.rio.NiFpga;
import com.sun.jna.Function;
import com.sun.jna.NativeLibrary;

import edu.wpi.first.wpilibj.RobotBase;

public class EmulatorControl {
	public static Function func = NativeLibrary
			.getInstance(NiFpga.LIBRARY_NAME).getFunction("StartEmulator");
	public static Function setTeam = NativeLibrary.getInstance(
			NiFpga.LIBRARY_NAME).getFunction("SetEmulatedTeam");

	public static void start(final int teamID, final Class<? extends RobotBase> base) {
		setTeam.invokeVoid(new Object[] { teamID });
		new Thread(new Runnable() {
			public void run() {
				System.out.println("StartEmulator");
				func.invokeInt(new Object[0]);
				System.out.println("StartEmulatorEnd");
			}
		}).start();
		try {
			base.newInstance().startApp();
		} catch (Exception e) {
			e.printStackTrace();
		}
	}
}
