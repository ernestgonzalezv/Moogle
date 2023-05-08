namespace MoogleEngine;
using System.Text.RegularExpressions;

public static class Moogle
{

    static string [] path = Directory.GetFiles(@"../Content", "*.txt"); //getting all the files in Content
    //dictionaries to calculate tf-idf
    static Dictionary<string, List<string> > d1 = new Dictionary<string, List<string> > (); //first one 
    static Dictionary<string,Dictionary<string,int>> tf = new Dictionary<string, Dictionary<string,int>>(); // frequency 
    static Dictionary<string,double> idf = new Dictionary<string,double> (); //inverse frequency 
    
    //building vocabulary to calculate levenshtein distance 
    static List<string> vocabulary  = new List<string> ();
    static Dictionary<string,bool> invocab = new Dictionary<string,bool> ();
    static List<string> wordsToChange = new List<string> (); //changing words of the query
    static List<string> closestWord = new List<string> (); //closest words of the ones that need to be changed
    //Added Details
    static List<string> NotOperator = new List<string> () ; //! not operator
    static List<string> HasToAppear = new List<string> (); // ^ hastoappear operator
    


     //methods 
    static string remove_punctuation(string a ){ // remove signs 
        return Regex.Replace(a,@"[^\w\d\s]","");
    }
    static string [] normalize_txt (string [] words){ // tolower () and remove punctuation for the whole array 
        int h=0 ; 
        string [] real_words = new string[words.Length];
        for(int k=0;k<words.Length;k++){
            if(words[k]!=""){
                real_words[h]=words[k].ToLower();
                real_words[h]=remove_punctuation(real_words[h]);
                h++;
            }
        }
        return real_words;
    }
    static (List<string> , Dictionary<string, int>) get_frequency(string [] words){ //this one returns vocabulary and tf_dictionary of the ith file  
        Dictionary<string,int> f = new Dictionary <string,int> (); //tf dictionary of the ith file
        Dictionary<string, bool> mark= new Dictionary<string,bool> (); //to not get the words more than once
        List<string> v = new List<string> (); //vocabulary of the ith file to Construct d1 in order to iterate in the cosine similarity method
        for(int k=0;k<words.Length;k++){ //word by word
            if(words[k]!=null && words[k]!=" "){ 

                //adding into vocabulary to proccess the levenshtein distance later
                if(!invocab.ContainsKey(words[k])){
                    invocab.Add(words[k],true);
                    vocabulary.Add(words[k]);
                }

                //idf
                if(!mark.ContainsKey(words[k])){ // not already marked 
                    mark.Add(words[k],true);
                    v.Add(words[k]); // adding to the vocabulary of the ith doc and not getting it more than once bc of the mark dict
                    //adding into idf 
                    if(!idf.ContainsKey(words[k])){ // not already in the dictionary 
                        idf.Add(words[k],1); //create a position for the word with key=1 
                    }else{ 
                        idf[words[k]]++; //just add 1 
                    }
                }
                //tf
                if(!f.ContainsKey(words[k])){ // if its not already in the tf dict, create a position for the word
                    f.Add(words[k],1);
                }else{ //if not then ++
                    f[words[k]]++;
                }    
            }
        }
        return (v,f); //returns a tuple with both the ith vocab and a tf dictionary for the ith index
    }
    static void query_tfidf(string [] q , Dictionary<string,double> tf_q , Dictionary<string,double> tfidf_q){ // building tfidf_q dictionary 
        //query tf
        for(int i=0;i<q.Length;i++){
            if(q[i]!="" && q[i]!=null){
                if(!tf_q.ContainsKey(q[i])){ //not already in the dict
                    tf_q.Add(q[i],1); //create a position 
                }else{ //if not then add 1
                    tf_q[q[i]]++; 
                }
            }
        }
        //query tf*idf
        for(int i=0;i<q.Length;i++){
            if(q[i]!=null && q[i]!="" && !tfidf_q.ContainsKey(q[i])){
                double idf_value; //variable for the idf value
                if(!idf.ContainsKey(q[i])){ //if the word isnt found in any document, its idf value is 0 
                    idf_value=0; 
                }else idf_value=Math.Log(path.Length/idf[q[i]]); // idf formula
                tfidf_q[q[i]]=tf_q[q[i]]*idf_value;
            }
        }
    }
    static List<(int,double,string)> calculate_cosine(string [] q, Dictionary<string,double> tfidf_q, Dictionary<string,double> tf_q){
        List<(int,double,string)> cosines = new List<(int,double,string)>(); // item1 is the index, item2 the score, item3 the snippets
        for(int i=0;i<path.Length;i++){
            string snipets = ""; //concatenate with every word that's in both query and the i-th document
            //calculate cosine betweem doc[i] and query
            double cosine; // summatory(ai*bi)/ (sqrt(sum(ai*ai)) * sqrt(sum(bi*bi)))
            double summatory1=0;// summatory of the square of  (tfidfdoc * tfidfquery)
            double querysum = 0 ; //sum of the squares of query's tfidfs
            double docsum = 0 ; // sum of the squares of the i-th doc's tfidfs 
            
            //just to be precise 
            double doc = 0 ; 
            double sq = 0 ; 


            for(int j = 0 ;j<d1[path[i]].Count;j++){ //iterating through ev single word of the ith doc
                
                double tfidfdoc= tf[path[i]][d1[path[i]][j]] * (Math.Log(path.Length/idf[d1[path[i]][j]])); 
                doc = tfidfdoc; //getting the doc's tfidf 
                docsum = docsum + Math.Pow(doc,2); //adding the tfidf squared
                if(tf_q.ContainsKey(d1[path[i]][j])){ //if the word is found in query, we can multiply the tfidfs and add it to the totalsum
                    snipets+=(d1[path[i]][j]+" "); //concatenate bc the word is an overlap
                    sq=tfidf_q[d1[path[i]][j]];
                    summatory1 = summatory1 + (doc*sq); //totalsum update
                }
            }
            for(int j = 0 ;j<q.Length;j++){ //iterating through every single word if the query in order to build the querysum
                if(q[j]!=null && q[j]!=" "){
                    querysum = querysum + Math.Pow(tfidf_q[q[j]],2); //querysum update
                }
            }

            if(summatory1 == 0 || querysum==0 || docsum==0)cosine=0; //(no overlaping words or empty doc or empty q) => cosine = 0 
            else {cosine = summatory1/Math.Sqrt(docsum*querysum);} //cosine similarity formula

            
            //Not Operator 
            for(int k=0;k<NotOperator.Count;k++){
                if(tf[path[i]].ContainsKey(NotOperator[k])){ //if the word appears then just dont return the doc by excluding the negative cosines 
                    cosine = -1;
                }
            }
            //Has to Appear Operator
            for(int k=0;k<HasToAppear.Count;k++){
                if(!tf[path[i]].ContainsKey(HasToAppear[k])){ //if the word doesnt appear then dont return the doc
                    cosine = -1 ; 
                }
            }
            if(cosine > 0 ){ //getting only positive (0,1] cosines to make the operators work
                cosines.Add((i,cosine,snipets));
            }
        }
        cosines = cosines.OrderBy(i => i.Item2).ToList(); //sorting the elements by score(item2) in ascending order
        cosines.Reverse(); //we wantem in descendent order
        return cosines;
    }


    //WORKING WITH SUGG PARAMETER:
    //method to calculate the minimum edit distance between two words with only 3 operations(deletion,replacement and insertion) with Levenshtein Distance
    static int Levenshtein(string a, string b) { //o(n*m)
        int [,] matrix = new int [a.Length+1,b.Length+1]; // matrix o(n+1,m+1)
        //case when one of the words is empty
        if(a.Length==0)return b.Length;
        if(b.Length==0)return a.Length;

        //initialize
        for(int i=0;i<=a.Length;i++){matrix[i,0] = i;}
        for(int j=0;j<=b.Length;j++){matrix[0,j] = j;}


        //calculate minimum edit distance 
        for(int i=1;i<=a.Length;i++){
            for(int j=1;j<=b.Length;j++){
                int c; // variable for the cost 
                if(b[j-1] == a[i-1]) c = 0 ;
                else c = 1;
                // now we can compute the minimum between replace, insert and delete operations :)
                matrix[i,j] = Math.Min(Math.Min(matrix[i-1,j]+1, matrix[i,j-1]+1),matrix[i-1,j-1] + c); 
            }
        }
        // return dp[n][m]
        return matrix[a.Length, b.Length];
    }
    //Method to update a query in order to get more results in the search ;)
    static void query_suggestion(string [] q){ // suggesting a better query 
        //finding closest words
        for(int i=0;i<wordsToChange.Count;i++){
            string currentWord= wordsToChange[i];
            string closest = "";
            int distance = int.MaxValue;
            //searching for the closest edit word
            for(int j=0;j<vocabulary.Count;j++){
                int l = Levenshtein(currentWord,vocabulary[j]);
                if(l<distance){ //upgrade distance and closest words once found a better distance 
                    distance=l;
                    closest=vocabulary[j];
                }
            }
            closestWord.Add(closest); // adding it to upgrade 
        }
        //changing query 
        int h =0 ; 
        for(int i=0;i<q.Length;i++){
            if(q[i]!=null && h<wordsToChange.Count && q[i] == wordsToChange[h]){
                q[i]=closestWord[h];
                h++;
            }
        }
    } 

    
    public static SearchResult Query(string query) {
        
        
        //query stuff
        string [] q = normalize_txt( query.Split(' ') ) ; 
        

        //filling operators' Lists
        for(int i=0;i<query.Split(' ').Length; i++){
            if(query.Split(' ')[i].Length>0 && query.Split(' ')[i][0]=='!'){
                NotOperator.Add(remove_punctuation(query.Split(' ')[i]).ToLower());
            } 
            if(query.Split(' ')[i].Length>0 && query.Split(' ')[i][0]=='^'){
                HasToAppear.Add(remove_punctuation(query.Split(' ')[i]).ToLower());
            }
        }

        //tfidf Dictionaries for the query 
        Dictionary<string,double> tf_q = new Dictionary<string,double> ();
        Dictionary<string,double> tfidf_q = new Dictionary<string, double>();


        //docs tfidf
        for(int i=0;i<path.Length;i++){ //iterate through ev single doc 
            if(!tf.ContainsKey(path[i])){ //ith doc already calculated
                string [] words = normalize_txt(File.ReadAllText(path[i]).Split(' '));
                Console.WriteLine(path[i]);
                (List<string> vocab , Dictionary<string,int> tfi) = get_frequency(words); //getting the i-th vocab and i-th tf respectively
                tf.Add(path[i], tfi); 
                d1.Add(path[i], vocab);
            }
        }
        
        //initialize queryVector
        query_tfidf(q, tf_q, tfidf_q);
        //calculate cosine 
        List<(int,double,string)> scores = calculate_cosine(q, tfidf_q,tf_q);
        //suggestion

        for(int i=0;i<q.Length;i++){//checking for words that aint in the corpus
            if(q[i]!=null && !idf.ContainsKey(q[i])){ // not in the corpus
                wordsToChange.Add(q[i]); // we will pass these through the suggestions method in case we dont get enough results 
            }
        }
        if(scores.Count<5 && wordsToChange.Count>0) { // not enough results because of words that arent in the corpus
            query_suggestion(q);
            scores.Clear(); //clean the scores
            tf_q.Clear(); //clean tf dictionary for query
            tfidf_q.Clear(); //clean tfidfs for query 
            query_tfidf(q,tf_q, tfidf_q); // compute the tfidf again 
            scores=calculate_cosine(q,tfidf_q,tf_q); //calculate the scores again 
        }
         //upgrading query for the suggestion parameter
        query="";
        for(int i=0;i<q.Length;i++){
            if(q[i]!=null){
                query+=(q[i]+(" "));
            }
        }
        SearchItem[] items = new SearchItem[Math.Min(scores.Count,5)]; // the minimum function bc we can get less than five docs in the search 
        for(int i=0;i<items.Length;i++){
            //create the  i-th item with the 3 parameters 
            items[i]=new SearchItem(Path.ChangeExtension(Path.GetFileName(path[scores[i].Item1]),null),scores[i].Item3,Convert.ToSingle(scores[i].Item2));
            Console.WriteLine(path[scores[i].Item1] + " " + scores[i].Item2);
        }
        
        //cleaning operators for future queries
        NotOperator.Clear();     
        HasToAppear.Clear();
        //cleaning suggestion tools
        wordsToChange.Clear();
        closestWord.Clear();
        
        return new SearchResult(items,query);
    }
}
