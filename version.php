<?php
	// En esta clase se establecen los filtros para el iterador.
	class MyRecursiveFilterIterator extends RecursiveFilterIterator {

		// Acá podemos evitar que ciertos archivos se listen.
	    public static $EXCLUDE = array(
	        'version.php',
	        'VersionInfo.json'
	    );

	    // Aca van las condiciones para la lista.
	    public function accept() {
	        return !in_array($this->current()->getFilename(), self::$EXCLUDE, true);
	    }

	}
	
	$serverfolder = '/' . $_GET["sv"] . '/';
	
	$subfolder = __DIR__.$serverfolder;
	
	/*
		Escanea los archivos que estan en el directorio actual y, aplicando el filto
		lista los archivos y los guarda en el array `files[numeroDeArchivo]`
	*/
	function listFiles($subfolder, $serverfolder) {
		$iterator 	= new RecursiveDirectoryIterator($subfolder);
		$iterator->setFlags(RecursiveDirectoryIterator::SKIP_DOTS);
		
		$filter 	= new MyRecursiveFilterIterator($iterator);
		$all_files  = new RecursiveIteratorIterator($filter, RecursiveIteratorIterator::SELF_FIRST);

		$file = array();
		$folders = array();
		$file_count = 0;
		$folder_count = 0;

		foreach ($all_files as $file) {
			$path_name = $file->getPathname();
			// Hacemos que sea un path relativo.
			$name = substr($path_name, strlen($subfolder));

			// Si es un directorio, no lo guardo en el array.
			if (is_dir($path_name)) {
				$folders[$folder_count] = $serverfolder . $name;
				$folder_count++;
			}
			else {
				$files[$file_count]['name'] = $serverfolder . $name;
				$files[$file_count]['checksum'] = md5_file($path_name);
				$file_count++;
			}
		}

		$manifest = array(
			'LauncherVersion' => md5_file("Launcher - ComunidadWinter.exe"),
			'TotalFiles' => $file_count,
			'TotalFolders' => $folder_count
		);

		return array(
			'Manifest' => $manifest,
			'Files' => $files,
			'Folders' => $folders
		);

	}

	// Transformamos el array que nos devuelve la funcion `listfiles()` a JSON
	$output = json_encode(listFiles($subfolder, $serverfolder), JSON_PRETTY_PRINT);
	
	// Guardamos el archivo.
	file_put_contents($subfolder . "VersionInfo.json", $output);
	
	header('Content-Type: application/json');
	// Mostramos lo que grabamos en el .json
	print_r($output);
?>