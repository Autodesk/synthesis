// package com.autodesk.synthesis.revrobotics;
// import java.lang.reflect.Constructor;

// import com.autodesk.synthesis.CANEncoder;
// import com.revrobotics.CANSparkBase;

// public class SparkAbsoluteEncoder extends com.revrobotics.SparkAbsoluteEncoder {
//     private CANSparkBase base;

//     // We're prettys sure that it's impossible to make a child class of this parent class with a constructor, because the parent's constructor is private
//     // Reflection didn't work since a child constructor __needs__ a super() call at the top of the body
//     // Passing in the constuctor wouldn't work either, since it's just a function pointer and the child constructor would have no idea that it points to its super
//     public SparkAbsoluteEncoder(CANSparkBase base, com.revrobotics.SparkAbsoluteEncoder.Type type) throws Exception {
//         try {
//             Constructor<com.revrobotics.SparkAbsoluteEncoder> constructor = com.revrobotics.SparkAbsoluteEncoder.class.getDeclaredConstructor(com.revrobotics.CANSparkBase.class, com.revrobotics.SparkAbsoluteEncoder.Type.class);
//             constructor.setAccessible(true);
//             com.revrobotics.SparkAbsoluteEncoder parent = constructor.newInstance(base, type);
//         } catch (Exception e) {
//             e.printStackTrace();
//         }
//     }

//     @Override
//     public double getPosition() {
//         return this.base.m_encoder.getPosition();
//     }

//     @Override
//     public double getVelocity() {
//         return this.base.m_encoder.getVelocity();
//     }
// }
