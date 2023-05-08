namespace MoogleEngine;

//la clase esta hecha para la evaluacion de algebra aunque no se usa en la implementacion 

public class Matrix{

    public double [,] m {get; private set; }
    public Matrix(double [,] a){
        this.m=a;
    }
    //metodo que recorre la matriz y nos devuelve su estado 
    public static void iterate(double [,] a){
        for(int i=0;i<a.GetLength(0);i++){ // ev single row
            for(int j=0;j<a.GetLength(1);j++){ //ev column 
                System.Console.WriteLine(a[i,j] + " ");
            }
            System.Console.WriteLine();
        }
    }

    //sumar matrices, itera posicion x posicion y la suma es asignada a una matriz resultante
    public static double[,] MatrixSum(double [,] a , double [,] b){
        double [,] ans = (double[,])a.Clone(); //matriz respuesta
        if(a.GetLength(0)!=b.GetLength(0) || a.GetLength(1)!=b.GetLength(1)){
            //no tienen igual dimensiones y no c pueden sumar 
            System.Console.WriteLine("no se pueden sumar");
            for(int i=0;i<ans.GetLength(0);i++){
                for(int j=0;j<ans.GetLength(1);j++){
                    ans[i,j] = int.MinValue;
                }
            }
            return ans;
        }
        for(int i=0;i<ans.GetLength(0);i++){
            for(int j=0;j<ans.GetLength(1);j++){
                ans[i,j] += b[i,j];
            }
        }
        return ans;
    }
    //restar matrices con mismo proccedure q con la suma 
    public static double[,] MatrixSubst(double [,] a , double [,] b){
        double [,] ans = (double[,])a.Clone(); //matriz respuesta
        if(a.GetLength(0)!=b.GetLength(0) || a.GetLength(1)!=b.GetLength(1)){
            //no tienen igual dimensiones y no c pueden restar 
            System.Console.WriteLine("no se pueden restar");
            for(int i=0;i<ans.GetLength(0);i++){
                for(int j=0;j<ans.GetLength(1);j++){
                    ans[i,j] = int.MaxValue;
                }
            }
            return ans;
        }
        for(int i=0;i<ans.GetLength(0);i++){
            for(int j=0;j<ans.GetLength(1);j++){
                ans[i,j] -= b[i,j];
            }
        }
        return ans;
    }
    //multplicacion de matrices con triple for 
    public static double[,] MatrixMult(double [,] a, double [,] b){
        double[,] ans = new double[a.GetLength(0), b.GetLength(1)];
        if(a.GetLength(1) != b.GetLength(0)){
            //no son multiplicables
            System.Console.WriteLine("NO SON MULTIPLICABLES");
            return ans;
        }
        for(int i=0;i<ans.GetLength(0);i++){
            for(int j=0;j<ans.GetLength(1);j++){
                double sum =0 ; //getting the sum of the product a[i,k] * b[k,j]
                for(int k=0;k<a.GetLength(1);k++){
                    sum+= (a[i,k] * b[k,j]);
                }
                ans[i,j] = sum ; 
            }
        }
        return ans;
    }
    //multiplicar la matriz por un escalar, se clona la matriz y c itera pos por pos multiplicando
    public static double[,] ScalarProduct(double [,] a , double scalar){
        double [,] ans = (double[,])a.Clone();
        for(int i=0;i<ans.GetLength(0);i++){
            for(int j=0;j<ans.GetLength(1);j++){
                ans[i,j] = ans[i,j] * scalar;
            }
        }
        return ans;
    }
    //devolver traspuesta convirtiendo cada fila en columna
    public static double [,] T(double[,] a){
        double [,] b = (double[,])a.Clone();
        double [,] c = new double [b.GetLength(1),b.GetLength(0)];
        for(int i=0;i<c.GetLength(0);i++){
            for(int j=0;j<c.GetLength(1);j++){
                c[i,j] = b[j,i];
            }
        }
        return c;
    }
    //metodo para devolver un menor y asi construir el determinante
    public static double[,] MatrixMenor(double [,] a, int row, int column){
        double [,] m = new double [a.GetLength(0)-1, a.GetLength(0)-1] ; //matriz para el menor 
        int j1=0;
        int i1=0;
        for(int i=0;i<a.GetLength(0);i++){
            //saltarse la fila en la fila q quiero eliminar
            if(i==row) continue; 
            for(int j=0;j<a.GetLength(0);j++){
                if(j==column)continue; //saltar columna a eliminar
                m[i1,j1] = a[i,j];
                j1++;
                if(j1>= a.GetLength(0)-1){ //posicionar de nuevo los idxs
                    i1++; //salto de fila
                    j1=0;
                }
            }
        }
        return m ; 
    }

    //metodo para devolver el determinante
    public static double Determinant (double [,] m){
        double determinant = 0 ; 
        if(m.GetLength(0)!=m.GetLength(1)){ // tiene q ser cuadrada
            System.Console.WriteLine("imposible calcular el determinante si no es cuadrada");
            return int.MaxValue;
        }
        int n = m.GetLength(1); // filas y columns
        if(n==1){
            determinant = m[0,0];
        }
        else if (n==2){
            determinant = (m[0,0] * m[1,1]) - (m[0,1]*m[1,0]);
        }
        else{ 
            for(int i=0;i<n;i++){
                double [,] a = MatrixMenor(m,0,i); // matriz para el menor
                determinant += (m[0,i] * Math.Pow(-1,i)*Determinant(a)); // recursividad con menores
            }

        }
        return determinant;
    }

    //metodo para devolver la inversa de una matriz
    //calcular la adjunta y multiplicarle el escalar 1/determinante de la matriz dada
    public static double [,] InverseMatrix(double [,] a){
        double determinant = Determinant(a) ;
        int tmn = (int)Math.Sqrt(a.Length);
        double [,] adj = new double [tmn,tmn] ; 
        for(int i=0;i<tmn;i++){
            for(int j=0;j<tmn;j++){
                double [,] m =MatrixMenor(a,i,j); //matrix para el menor 
                if((i+j)%2==0){
                    adj[i,j] = Determinant(m);
                }else{
                    adj[i,j] = -1 * Determinant(m);
                }

            }
        }
        return ScalarProduct(adj, 1/determinant);
    }
}