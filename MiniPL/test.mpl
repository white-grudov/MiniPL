var nTimes : int := 0;
print "How many times? ";
read nTimes;
var x : int;
for x in 0..nTimes-1 do
	print x;
	print " : Hello, World!\n";
end for;
x := x + 1;
if x = nTimes do
	print "x is equal to ntimes";
end if;
