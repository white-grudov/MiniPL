print "Sentence builder\n";
print "Enter the number of words: ";
var words : int;
read words;
var current : string;
var sentence : string := "";
var i : int;
for i in 1..words do
	print "Enter ";
	print i;
	print " word: ";
	read current;
	sentence := sentence + current;
	sentence := sentence + " ";
end for;
print "The sentence is: " + sentence;
print "\n";