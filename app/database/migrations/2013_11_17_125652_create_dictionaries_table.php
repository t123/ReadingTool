<?php

use Illuminate\Database\Schema\Blueprint;
use Illuminate\Database\Migrations\Migration;

class CreateDictionariesTable extends Migration {

    /**
     * Run the migrations.
     *
     * @return void
     */
    public function up() {
        Schema::create('dictionaries', function(Blueprint $table) {
            $table->increments('id');
            $table->string('name', 25);
            $table->string('encoding', 10);
            $table->string('windowName', 25);
            $table->string('url', 100);
            $table->boolean('sentence');
            $table->boolean('autoOpen');
            $table->integer('language_id')->unsigned();
            
            $table->foreign('language_id')->references('id')->on('languages');
        });
    }

    /**
     * Reverse the migrations.
     *
     * @return void
     */
    public function down() {
        Schema::drop('dictionaries');
    }

}
