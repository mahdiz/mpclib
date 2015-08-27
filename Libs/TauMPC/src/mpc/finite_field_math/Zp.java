/*
 * To change this template, choose Tools | Templates
 * and open the template in the editor.
 */

package mpc.finite_field_math;

import java.util.List;
import mpc.circuit.Gate.Operation;


public class Zp {
    public final int prime;
    private int num;

    public Zp(int prime, int num) {
        this.prime = prime;
        this.num = modulo(num , prime);
    }

    public Zp(Zp toCopy) {
        this.prime = toCopy.prime;
        this.num = modulo(toCopy.num , this.prime);
    }
    
    public Zp add(Zp operand2){
        num = modulo(num + operand2.getValue()) ;
        return this;        
    }

  public Zp constAdd(Zp operand2){
        Zp temp = new Zp(this);
        temp.num = modulo(num + operand2.getValue());
        return temp;
    }


    public Zp addListContent(List<Zp> zpList){
        for (Zp zp : zpList){
            this.add(zp);
        }
        return this;
    }

    public Zp sub(Zp operand2){
        num = modulo(num - operand2.getValue()) ;
        return this;
    }

    public Zp constSub(Zp operand2){
        Zp temp = new Zp(this);
        temp.num = modulo(num - operand2.getValue());
        return temp;
    }


    public Zp divide(Zp operand2){
        if (operand2.num == 0)
            throw new IllegalArgumentException( "Cannot divide in zero" );
        num = modulo(num * getFieldInverse(operand2.getValue()) ) ;
        return this;
    }

   public Zp constDvide(Zp operand2){
       if (operand2.num == 0)
            throw new IllegalArgumentException( "Cannot divide in zero" );
        Zp temp = new Zp(this);
        temp.num = modulo(num * getFieldInverse(operand2.getValue()) ) ;
        return temp;
    }


   public  int  getFieldInverse(int fieldNum){
       int temp;
       fieldNum = modulo(fieldNum);
       for (int j=1; j < prime; j++)
       {
               temp = modulo(fieldNum  *  j) ;
               if  (temp == 1)
                       return j;
       }
        return  0;
    }


    public Zp mul(Zp operand2){
        num = modulo(num * operand2.getValue());
        return this;        
    }

    public Zp constMul(Zp operand2){
        Zp temp = new Zp(this);
        temp.num = modulo(num * operand2.getValue());
        return temp;
    }
    
    public Zp Calculate(Zp operand2, Operation operation){
        switch(operation){
            case ADD:
                return add(operand2);
            case MUL:
                return mul(operand2);
           case SUB:
               return sub(operand2);
            case DIV:
                return divide(operand2);
            default:
                assert false;
                throw new RuntimeException("Unknown operation type " + operation);
        }        
    }
        
    public int getValue() {
        return num;
    }

    public void setValue(int newNum) {
        num = modulo(newNum);
    }


    public  int  modulo(int i){
            return  modulo(i , prime);
    }

   public static int  modulo(int i, int prime){
        int temp = i;
        temp = temp % prime;
            if (temp < 0)
                temp += prime;

        return temp;
   }


  public static boolean  isPrime(int num){
        int temp;
        for (int i=2 ; i <= (int) Math.sqrt(num); i++)
        {
                temp = num / i;

                if ( num == (temp * i))
                {
                    return false;
                }
        }
        return true;
  }


 public static int[] getFieldInverseArr(int prime){
        int temp;
        int[] invArr = new int[prime];

        for (int i=0; i< prime; i++)
            for (int j=0; j < prime; j++)
            {
                   temp = modulo(i * j, prime) ;
                   if  (temp == 1)
                   {
                       invArr[i] = j;
                       break;
                   }           
          }
        return invArr;
    }

     public static int  getFieldMinimumPrimitive(int prime){
         int  w_i;
         boolean cond;
         boolean[] fieldElements = new boolean[prime];

        for (int w = 2; w < prime; w++)
        {
                w_i = 1;
                cond = true;
                for (int i = 1; i < prime; i++)
                {
                        fieldElements[i] = false;
                }

                for (int i = 1; i < prime; i++)
                {
                        w_i = modulo(w * w_i, prime);
                        if  (fieldElements[w_i])
                        {
                            cond = false;
                            break;
                        }
                        fieldElements[w_i]  = true;
                }

                if (cond)
                {
                    return w;
                }
        }
        
        throw new IllegalArgumentException("Cannot find field primitive for a field from non prime number.");
     }


     public static int calculatePower(int base , int exp, int prime) {
         int w_i = 1;
         for (int i = 0; i < exp; i++)
         {
                w_i = modulo(base * w_i, prime);
         }
         return w_i;
     }


    public static Zp evalutePolynomialAtPoint(List<Zp> polynomial, Zp point){
         int evaluation = 0;
        for (int i =0; i < polynomial.size();i++){
            evaluation += polynomial.get(i).getValue() * calculatePower(point.getValue(), i, point.prime);
        }
         return new Zp(point.prime, evaluation);
    }
    

    @Override
    public String toString() {
        return String.valueOf(num);
    }

    @Override
    public boolean equals(Object obj) {
        if (!(obj instanceof Zp)){
            return false;
        }
        Zp compare = (Zp)obj;
        return getValue() == compare.getValue() && prime == compare.prime;
    }

    @Override
    public int hashCode() {
        return new Integer(getValue()).hashCode();
    }

    public static void main(String[] args) {
        int prime = 233;

        Zp num1 = new Zp (prime, 4);
        Zp num2 = new Zp (prime, 6);
        int primitive = getFieldMinimumPrimitive(prime);
        System.out.println(primitive);

        int w_i = 1;
        for (int i = 1; i < prime; i++)
        {
                System.out.print( w_i + ", ");
                w_i = modulo(primitive * w_i, prime);
        }
        System.out.println( );


    }


}
