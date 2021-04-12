<?php

	// Le pongo una simple contraseña para evitar cambios innecesarios.
	if (!isset($_GET['pass']) || $_GET['pass'] !== 'WinterLwK2021') {
		die("<strong>Acceso Restringido!<strong>");
	}

	// En esta clase se establecen los filtros para el iterador.
	class MyRecursiveFilterIterator extends RecursiveFilterIterator {

		// Acá podemos evitar que ciertos archivos se listen.
	    public static $EXCLUDE = array(
	        'VersionInfo.json',
			'VersionInfo_back.json',
			'version.php',
			'AAMD532.DLL',
	    );

	    // Aca van las condiciones para la lista.
	    public function accept() {
			$filterExcludedFiles = !in_array($this->current()->getFilename(), self::$EXCLUDE, true);
			$excludeThisScript = $this->current()->getFilename() !== basename(__FILE__);
	        
			return $filterExcludedFiles && $excludeThisScript;
	    }

	}

	/*
		Escanea los archivos que estan en el directorio actual y, aplicando el filto
		lista los archivos y los guarda en el array `files[numeroDeArchivo]`
	*/
	function listFiles() {
		
		$iterator 	= new RecursiveDirectoryIterator('WinterClient');
		$iterator->setFlags(RecursiveDirectoryIterator::SKIP_DOTS);
		
		$filter 	= new MyRecursiveFilterIterator($iterator);
		$all_files  = new RecursiveIteratorIterator($filter,RecursiveIteratorIterator::SELF_FIRST);

		$output = array();
		$fileCount = 0;
		$folderCount = 0;

		foreach ($all_files as $file) {
			
			// Hacemos que sea un path relativo.
			$outputPathname = substr($file->getPathname(), strlen('WinterClient'));
			
			// Si es un directorio, guardo solo su nombre.
			if (is_dir($file->getPathname())){
				// Registramos la carpeta para que el launcher la cree.
				$output["Folders"]["Folder" . $folderCount]["Name"] = $outputPathname;
				
				// Mantenemos la cuenta de las carpetas listadas.
				$folderCount++;
				
				// Nos vamos del loop.
				continue;
			}

			// Mantenemos la cuenta de archivos listados.
			$fileCount++;

		  	$output["Files"]["File" . $fileCount]['Name'] = str_replace("/", "\\", $outputPathname);
		  	$output["Files"]["File" . $fileCount]['Checksum'] = md5_file($file->getPathname());
		}

		$output['Manifest']['TotalFiles'] = $fileCount;
		$output['Manifest']['TotalFolders'] = $folderCount;
		$output['Manifest']['LauncherVersion'] = md5_file("Launcher - ComunidadWinter.exe");

		return $output;

	}

	header('Content-Type: application/json');
	// Transformamos el array que nos devuelve la funcion `listfiles()` a JSON
	$output = json_encode(listFiles(), JSON_PRETTY_PRINT);

	// Guardamos el archivo.
	file_put_contents("VersionInfo.json", $output);

	// Mostramos lo que grabamos en el .json
	echo $output;
?>