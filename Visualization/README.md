# Installation

Open your terminal and navigate into this directory. 

Install `python3.8` or newer (getting from [Python](https://www.python.org/downloads/))

Execute the following command:

```bash
pip3 install -r requirements.txt
```

# Start

Start the mini visualization by calling:

```bash
python3 main.py
```
or:
```bash
python main.py
```

Start the simulation and activate the visualization output in your configuration by setting the field `Visualization` to `true`

```json
"layers": [
    {
      "name": "MazeLayer",
      "file": "Resources/grid.csv",
      "mapping": [
        {
          "parameter": "Visualization",
          "value": true
        },
  // ... your agent, entities and layer mappings
}
```

