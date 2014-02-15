function prompt(n, t){
	this.name = n;
	this.tag = t;
};
function promptGenerator() { }
promptGenerator.prototype.prompts = [
	new prompt("a thread", "thread"),
	new prompt("a staple", "staple"),
	new prompt("a piece of printing paper", "pieceofpaper"), 
	new prompt("a toothbrush", "toothbrush"),
	new prompt("a television", "television"),
	new prompt("a phone", "phone"),
	new prompt("a picture frame", "pictureframe"),
	new prompt("a bed", "bed"), 
	new prompt("a couch", "couch"), 
	new prompt("a table", "table"), 
	new prompt("a chair", "chair"),
	new prompt("a candle", "candle"), 
	new prompt("a lamp", "lamp"), 
	new prompt("a cup", "cup"), 
	new prompt("a pencil", "pencil"), 
	new prompt("a flower", "flower"), 
	new prompt("a clock", "clock"), 
	new prompt("a box", "box"), 
	new prompt("a book", "book"), 
	new prompt("a CD", "cd"), 
	new prompt("a window", "window"),
	new prompt("water", "water"),
	new prompt("a shoe", "shoe"),
	new prompt("an umbrella", "umbrella"),
	new prompt("a key", "key"),
	new prompt("a knife", "knife"),
	new prompt("a match", "match"),
	new prompt("a paper clip", "paperclip"),
	new prompt("a leaf", "leaf")
];
promptGenerator.prototype.sample = function(n) {
	return _.sample(this.prompts, n);
};

promptGenerator.prototype.makeTag = function(items) {
	var parts = ["lycb"], delimiter = "-";
	for(var i = 0; i < items.length; i++) {
		parts.push(items[i].tag);
	}
	return parts.join(delimiter);
};
