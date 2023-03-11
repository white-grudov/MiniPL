print "Sum of n numbers\n";
print "Enter n: ";
var n : int;
read n;
var sum : int := 0;
var i : int;

if n > 0 do
    for i in 1..n do
        sum := sum + i;
    end for;
    print "The sum of ";
    print n;
    print " numbers is: ";
    print sum;
else
    print "Number is less than 1!";
end if;