package mpc.finite_field_math;

import java.util.ArrayList;
import java.util.Arrays;
import java.util.LinkedList;
import java.util.List;

/*
 * To change this template, choose Tools | Templates
 * and open the template in the editor.
 */
import java.util.Map;
import java.util.TreeMap;


public class Polynom
{
    private List<Zp>    Coeffncies;

    public Polynom(List<Zp> Coeffncies)
    {
        init(new ArrayList<Zp>(Coeffncies));
    }
    
    public Polynom(Zp[] Coeffncies)
    {
        init(new ArrayList<Zp>(Arrays.asList(Coeffncies)));
    }

    public Polynom(int[] Coeffncies, int CoefficinesFieldSize)
    {
        ArrayList<Zp> TempList = new ArrayList<Zp>();
        for (int i=0; i<Coeffncies.length; i++)
        {
            TempList.add(new Zp(CoefficinesFieldSize,Coeffncies[i]));
        }
        init(TempList);
    }
    
    private void init(ArrayList<Zp> Coeffncies){
        assert Coeffncies.size() > 0;
        this.Coeffncies = new ArrayList<Zp>();          
        for (int i = Coeffncies.size() - 1; i >=0; i--){
            if (Coeffncies.get(i).getValue() != 0 || this.Coeffncies.size() > 0){
                this.Coeffncies.add(0, Coeffncies.get(i));               
            }
        }        
    }
    
    public int getCoefficinesFieldSize()
    {
        if (Coeffncies.size() == 0){
            throw new RuntimeException("Polynom is empty");
        }
        return Coeffncies.get(0).prime;
    }

    public List<Zp> getCoeffncies()
    {
        return Coeffncies;
    }
    
    public int getDegree()
    {
        return Coeffncies.size() - 1;
    }
    
    /**
     * @param SamplePoint - the desired sampling point.
     * @return  the result of the polynom when replacing variable ("x") with the sampling point
     */
    public Zp Sample(Zp SamplePoint)
    {
        if (Coeffncies.size() == 0){
            return null;
        }
        final int CoefficinesFieldSize = getCoefficinesFieldSize();
        /* The initialized sum is 0 */
        Zp Sum = new Zp(CoefficinesFieldSize,0);
        
        for(int i=0; i < Coeffncies.size(); i++)
        {
            /* replace each "Ai*x^i" with "Ai*SamplePoint^i" */
            Zp Xi = new Zp (CoefficinesFieldSize,
                            Zp.calculatePower(SamplePoint.getValue(), i, CoefficinesFieldSize));
            Zp Ai = new Zp(CoefficinesFieldSize, Coeffncies.get(i).getValue());
          
            Zp AiXi = Xi.mul(Ai);
            
            /* Sum all these values(A0+A1X^1+...AnX^n) */
            Sum = Sum.add(AiXi);
        }
        
        return Sum;
    }
    
    public Polynom divideWithRemainder( Polynom p) {         
        if (Coeffncies.size() == 0 || p.Coeffncies.size() == 0){
            return null;//null
        }
        Polynom[] answer = new Polynom[2];    
        int prime = Coeffncies.get(0).prime;
        int m = getDegree();         
        int n = p.getDegree();         
        if ( m < n ){                 
            return null;       
        }         
        Zp[] quotient = new Zp[ m - n + 1];         
        Zp[] coef = new Zp[ m + 1];         
        for ( int k = 0; k <= m; k++ )                 
            coef[k] = new Zp(Coeffncies.get(k));         
        Zp norm = new Zp(prime, p.Coeffncies.get(n).getFieldInverse(p.Coeffncies.get(n).getValue()));
        for ( int k = m - n; k >= 0; k--){                 
            quotient[k] = new Zp(prime, coef[ n + k].getValue() * norm.getValue());                 
            for ( int j = n + k - 1; j >= k; j--)                         
                coef[j] = new Zp(prime, coef[j].getValue() - quotient[k].getValue() * p.Coeffncies.get(j-k).getValue());
        }         
        Zp[] remainder = new Zp[n];         
        for ( int k = 0; k < n; k++)                 
            remainder[k] = new Zp(coef[k]);         
        answer[0] = new Polynom( quotient);         
        answer[1] = new Polynom( remainder);  
        for (Zp zp : answer[1].Coeffncies){
            if (zp.getValue() != 0){
                return null;
            }
        }
        return answer[0];
    } 
        
    /**
     *  TBD::::: naive soultion, find a better solution from web 
     * @return the roots of the polynom (comparing the polynom to 0).
     */
    public List<Zp> GetRoots()
    {
        
        List<Zp> SolutionsList = new LinkedList<Zp>();
        
        /* BF - go over all the items in the field. and check if they solve the poly */
        for(int i=0 ; i < getCoefficinesFieldSize() ; i++)
        {
            Zp CurrentSamplePoint = new Zp(getCoefficinesFieldSize(),i);
            if (this.Sample(CurrentSamplePoint).getValue() == 0)
            {
                SolutionsList.add(CurrentSamplePoint);
            }
        }
        
        return SolutionsList;
    }
    
    @Override
    public String toString()
    {
        StringBuilder string=new StringBuilder();
        for (int i=0; i<getDegree(); i++)
        {
            string.append(Coeffncies.get(i));
            if (i < getDegree() - 1){
                string.append(", ");
            }
        }
        return string.toString();
    }

    @Override
    public boolean equals(Object obj) {
        if (!(obj instanceof Polynom)){
            return false;
        }
        Polynom p = (Polynom) obj;
        if (getDegree() != p.getDegree()){
            return false;
        }
        for (int i = 0; i <= getDegree(); i++){
            if (!(Coeffncies.get(i).equals(p.Coeffncies.get(i)))){
                return false;
            }            
        }
        return true;
    }
            
    public static Polynom hanfetzPolynom(int degree, int prime){
        List<Zp> coeffs = new ArrayList<Zp>();
        for (int i = 0; i <= degree; i++){
            int num = (int)(Math.random() * 100);
            coeffs.add(new Zp(prime, num));
        }
        return new Polynom(coeffs);    
    }
    
    public Polynom multiply(Polynom p){
        assert getDegree() > p.getDegree() && Coeffncies.size() > 0;
        int prime = Coeffncies.get(0).prime;
        Map<Integer, Zp> coeffs = new TreeMap<Integer, Zp>();
        for (int deg1 = 0; deg1 <= getDegree(); deg1++){
            for (int deg2 = 0; deg2 <= p.getDegree(); deg2++){ 
                int deg = deg1 + deg2;
                Zp curr = coeffs.get(deg);
                Zp newValue = new Zp(prime, Coeffncies.get(deg1).getValue() * p.Coeffncies.get(deg2).getValue());
                if (curr == null){
                    curr = newValue;                    
                }else{
                    curr = new Zp(prime, curr.getValue() + newValue.getValue());
                }
                coeffs.put(deg, curr);
            }           
        }
        return new Polynom(new ArrayList<Zp>(coeffs.values()));
    }
    
    public static void main(String[] args){
        int prime = 233;
        for (int degree = 4; degree < 100; degree++){
            Polynom p1 = hanfetzPolynom(degree, prime);
            int degree2 = Math.max(3, degree - 15);
            Polynom p2 = hanfetzPolynom(degree2, prime);
            Polynom p3 = p1.multiply(p2);
            if (degree %5==0){
                p3.Coeffncies.get(0).setValue(p3.Coeffncies.get(0).getValue() + 1);                
            }
            Polynom p11 = p3.divideWithRemainder(p2);
            boolean check = degree % 5 == 0 && p11 == null || p11 != null && p11.equals(p1);
            System.out.println("Test for degree " + degree + " and degree  " +  degree2 + ": " + (check ? "passed" : "failed"));
            if (!check){
                System.out.println("p1: " + p1.toString());
                System.out.println("p2: " + p2.toString());
                System.out.println("p3: " + p3.toString());
            }
            final Zp zpSample = new Zp(p1.getCoefficinesFieldSize(), 3);
            Zp erezResult = Zp.evalutePolynomialAtPoint(p1.getCoeffncies(),zpSample);
            Zp barResult = p1.Sample(zpSample);
            if (!erezResult.equals(barResult)){
                System.out.println("Erez: " + erezResult + "; Bar: " + barResult);                
            }            
        }
        System.out.println("Test Ended");
    }       
}
