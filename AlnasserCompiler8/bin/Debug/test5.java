class classone{
  public void firstclass(){
    int a,b,c;
    a = 5;
    b = 10;
    c = classone.secondclass(a, b);
    write("c = ");
    writeln(c);
    return;
  }
  public int secondclass(int a, int b){
    int c;
    c=a*b;
    return c;
  }
}
final class Main{
  public static void main(String [] args){
    classone.firstclass();
  }
}
