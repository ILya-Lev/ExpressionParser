# ExpressionParser
Grabs file with expression and returns a list of subexpressions in order of evalueation

The expression could contain only this set of operators:
"+-><="

"&"

"|"

each row contains operators with the same priority (from higher to lower)
also priority could be changed with parenthesises

variables may be composed of any symbols except operators and symbol '@' (the last one is used in implementation mechanism)

to process an expression you want - change content of expression.txt and run the app with command line: recursive_descent_parser.exe expression.txt

otherwise hardcoded expression will be parsed

summary

time of creation - about 3 days (up to 6h per day) from scratch (the main challenge was an algorithm - it was designed without any external help - internet and so on).
